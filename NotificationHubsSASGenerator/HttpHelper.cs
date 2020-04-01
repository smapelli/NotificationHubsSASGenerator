using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NotificationHubsSASGenerator
{
    internal class HttpHelper
    {
        const string USER_AGENT = "User-Agent: Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko";
        const string HTTP_POST = "POST";

        Form _parent;


        public HttpHelper(MainForm parent)
        {
            _parent = parent;
        }

        public async Task<string> DoPost(string url, string authHeader, string body)
        {
            string responseBody = null;

            Uri requestUri = new Uri(url);

            HttpWebRequest req = HttpWebRequest.Create(requestUri) as HttpWebRequest;
            req.Method = HTTP_POST;


            req.Headers["Authorization"] = authHeader;
            req.ContentType = "application/json";

            byte[] byteArray = Encoding.UTF8.GetBytes(body);
            req.ContentLength = byteArray.Length;

            Exception exc = null;
            try
            {
                // Get the request stream.
                using (Stream dataStream = await req.GetRequestStreamAsync())
                {
                    // Write the data to the request stream.
                    await dataStream.WriteAsync(byteArray, 0, byteArray.Length);
                    dataStream.Close();
                }
            }
            catch (Exception ex)
            {
                exc = ex;

                string msg = ex.Message;

                if (ex is WebException)
                {
                    WebException wex = ex as WebException;

                    if (wex.Response != null)
                    {
                        msg += Environment.NewLine + Environment.NewLine + GetResponseBody(wex);
                    }
                }

                DisplayMessage.Error(_parent, msg);
            }

            if (exc == null)
            {
                try
                {
                    // WEB RESPONSE

                    Task<WebResponse> task = req.GetResponseAsync();

                    //using (HttpWebResponse res = req.GetResponse() as HttpWebResponse)
                    using (HttpWebResponse res = await task as HttpWebResponse)
                    {
                        HttpStatusCode statusCode = res.StatusCode;
                        string statusDescription = res.StatusDescription;

                        string characterSet = res.CharacterSet;
                        string contentEncoding = res.ContentEncoding;
                        long contentLength = res.ContentLength;
                        string contentType = res.ContentType;

                        DateTime lastModified = res.LastModified;

                        string cerverResponse = null;
                        using (Stream dataStream = res.GetResponseStream())
                        {
                            // Reading the server response.
                            cerverResponse = await ReadToEndAsync(characterSet, dataStream);

                            dataStream.Close();
                        }

                        StringBuilder sb = new StringBuilder();

                        sb.AppendLine($"POST {url}");
                        sb.AppendLine($"HTTP status: {(int)statusCode} {statusCode}");
                        sb.Append(Environment.NewLine);

                        foreach (string key in req.Headers.Keys)
                        {
                            sb.AppendLine($"{key}: {req.Headers[key]}");
                        }

                        sb.Append(Environment.NewLine);
                        sb.AppendLine($"Response: {cerverResponse}");

                        responseBody = sb.ToString();

                        res.Close();
                    }
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;

                    if(ex is WebException)
                    {
                        WebException wex = ex as WebException;

                        if (wex.Response != null)
                        {
                            msg += Environment.NewLine + Environment.NewLine + GetResponseBody(wex);
                        }
                    }

                    DisplayMessage.Error(_parent, msg);
                }
            }

            return responseBody;
        }

        private async Task<string> GetResponseBody(WebException wex)
        {
            string body = null;

            if (wex.Response != null)
            {
                body = await ReadToEndAsync(wex.Response.ContentType, wex.Response.GetResponseStream());
            }

            return body;
        }

        private async Task<string> ReadToEndAsync(string charSet, Stream dataStream)
        {
            string response = null;

            Encoding encoding = GetEncoding(charSet);

            // Open the stream using a StreamReader for easy access.
            using (StreamReader reader = new StreamReader(dataStream, encoding))
            {
                // Read the server response.

                StringBuilder sb = new StringBuilder();
                char[] buffer = new char[1024];

                do
                {
                    int count = await reader.ReadAsync(buffer, 0, buffer.Length);
                    sb.Append(buffer, 0, count);
                }
                while (reader.EndOfStream == false);

                reader.Close();

                response = sb.ToString();
            }

            return response;
        }

        private Encoding GetEncoding(string charSet)
        {
            Encoding encoding = null;

            if (charSet != null && charSet.Length > 0)
            {
                try
                {
                    encoding = Encoding.GetEncoding(charSet);
                }
                catch (Exception ex)
                {
                    encoding = Encoding.UTF8;
                }
            }
            else
            {
                encoding = Encoding.UTF8;
            }

            return encoding;
        }
    }
}
