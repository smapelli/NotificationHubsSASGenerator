using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace NotificationHubsSASGenerator
{
    public partial class MainForm : Form
    {
        const string HUB_NAMESPACE = "{NOTIF_HUB_NS}";
        const string HUB_NAME = "{NOTIF_HUB_NAME}";

        string urlTemplate;

        string hubName;
        string hubNamespace;
        TimeSpan ttl = TimeSpan.Zero;
        string connStr;


        public MainForm()
        {
            InitializeComponent();

            comboBox1.Items.Add(new ComboBoxItem<int>("Days", 1));
            comboBox1.Items.Add(new ComboBoxItem<int>("Hours", 2));
            comboBox1.Items.Add(new ComboBoxItem<int>("Minutes", 3));

            comboBox1.SelectedIndex = 0;

            textBox3.Text = "1";

            new ToolTip().SetToolTip(textBox1, "Notification Hub Namespace (required)");
            new ToolTip().SetToolTip(textBox2, "Notification Hub Namespace (required)");
            new ToolTip().SetToolTip(textBox3, "Notification Hub Namespace (required)");
            new ToolTip().SetToolTip(textBox4, "Notification Hub Namespace (required)");

            new ToolTip().SetToolTip(textBox5, "Notification Hub Namespace (required)");
            new ToolTip().SetToolTip(textBox6, "Notification Hub Namespace (required)");
            new ToolTip().SetToolTip(textBox7, "Notification Hub Namespace (required)");
            new ToolTip().SetToolTip(textBox9, "Notification Hub Namespace (required)");
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            bool exists = ConfigurationManagerHelper.CheckConfigFileIsPresent();

            if (exists == false)
            {
                button1.Enabled = false;
                button2.Enabled = false;

                DisplayMessage.Error(this, "Configuration file not found. The application may not work.");
            }
            else
            {
                urlTemplate = ConfigurationManager.AppSettings["UrlTemplate"];

                if (urlTemplate != null)
                {
                    textBox6.Text = urlTemplate;
                }
                else
                {
                    button1.Enabled = false;
                    button2.Enabled = false;

                    DisplayMessage.Error(this, "Configuration file does not contain AppSettings key \"UrlTemplate\". The application may not work.");
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (ValidateInput())
            {
                string[] parts = connStr.Split(';');

                Dictionary<string, string> dictionary = new Dictionary<string, string>();

                foreach (string part in parts)
                {
                    if (part.Contains("Endpoint="))
                    {
                        dictionary.Add("Endpoint", part.Replace("Endpoint=", null));
                    }
                    else if (part.Contains("SharedAccessKeyName="))
                    {
                        dictionary.Add("SharedAccessKeyName", part.Replace("SharedAccessKeyName=", null));
                    }
                    else if (part.Contains("SharedAccessKey="))
                    {
                        dictionary.Add("SharedAccessKey", part.Replace("SharedAccessKey=", null));
                    }
                }


                if (dictionary.Count == 3)
                {
                    // URL format obtained by tracing a call made by the client returned by NotificationHubClient.CreateClientFromConnectionString() when calling SendTemplateNotificationAsync()

                    string url = urlTemplate;

                    url = url.Replace("{NOTIF_HUB_NS}", hubNamespace);
                    url = url.Replace("{NOTIF_HUB_NAME}", hubName);

                    // Microsoft.Azure.NotificationHubs.dll
                    //string sas = SharedAccessSignatureTokenProvider.GetSharedAccessSignature(dictionary["SharedAccessKeyName"], dictionary["SharedAccessKey"], url, ttl);

                    string sas = SharedAccessSignatureBuilder.GetSharedAccessSignature(dictionary["SharedAccessKeyName"], dictionary["SharedAccessKey"], url, ttl);

                    textBox5.Text = sas;
                }
                else
                {
                    DisplayMessage.Error(this, "An Azure Notification Hub connection string should have the \"Endpoint=...;SharedAccessKeyName=...;SharedAccessKey=...\" format.");
                }
            }
        }

        private bool ValidateInput()
        {
            // INPUT DATA READING

            hubName = textBox1.Text.Trim();
            hubNamespace = textBox2.Text.Trim();

            string str = textBox3.Text.Trim();

            ttl = TimeSpan.Zero;

            if (str != string.Empty && IsNumber(str))
            {
                switch (((ComboBoxItem<int>)comboBox1.SelectedItem).Value)
                {
                    case 1:
                        ttl = TimeSpan.FromDays(int.Parse(str));
                        break;
                    case 2:
                        ttl = TimeSpan.FromHours(int.Parse(str));
                        break;
                    case 3:
                        ttl = TimeSpan.FromMilliseconds(int.Parse(str));
                        break;
                }
            }

            connStr = textBox4.Text.Trim();


            // INPUT DATA VALIDATION

            if (hubName == string.Empty)
            {
                DisplayMessage.Error(this, "Provide the Azure Notification Hub name");
                return false;
            }

            if (hubNamespace == string.Empty)
            {
                DisplayMessage.Error(this, "Provide the Azure Notification Hub namespace");
                return false;
            }

            if (ttl == TimeSpan.Zero)
            {
                DisplayMessage.Error(this, "Token TTL must be an integer");
                return false;
            }

            if (connStr == string.Empty)
            {
                DisplayMessage.Error(this, "Provide the Azure Notification Hub connection string");
                return false;
            }

            return true;
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            if (textBox4.Text == string.Empty)
            {
                DisplayMessage.Error(this, "You need to generated a SAS before");
                return;
            }

            textBox8.Text = null;

            string url = urlTemplate;

            if (urlTemplate.Contains(HUB_NAME))
            {
                url = url.Replace(HUB_NAME, hubName);
            }
            else
            {
                DisplayMessage.Error(this, $"Placeholder {hubName} not found in \"UrlTemplate\" configuration item");
                return;
            }

            if (urlTemplate.Contains(HUB_NAMESPACE))
            {
                url = url.Replace(HUB_NAMESPACE, hubNamespace);
            }
            else
            {
                DisplayMessage.Error(this, $"Placeholder {hubNamespace} not found in \"UrlTemplate\" configuration item");
                return;
            }

            // SharedAccessSignature sr={URI}&sig={HMAC_SHA256_SIGNATURE}&se={EXPIRATION_TIME}&skn={KEY_NAME}

            //SharedAccessSignature sr=https%3a%2f%2fstefama-eh.servicebus.windows.net%2fstefamaeventhub%2fpublishers%2fabc&sig=4AI5vU5LmKk0V%2f71Swt4jmJkEU1uYajBKI6p3aSAUPg%3d&se=1531839083&skn=RootManageSharedAccessKey

            HttpHelper http = new HttpHelper(this);
            textBox8.Text = await http.DoPost(url, textBox5.Text, textBox7.Text);
        }

        private bool IsNumber(string s)
        {
            bool value = true;

            foreach (char c in s.ToCharArray())
            {
                value = value && char.IsDigit(c);
            }

            return value;
        }

        /// <summary>
        /// timestamp the message and add to the textbox.
        /// To prevent runtime faults should the amount
        /// of data become too large, trim text when it reaches a certain size.
        /// </summary>
        /// <param name="text"></param>
        public void AppendTrace(RichTextBox richtextbox, string text, Color textcolor)
        {
            // keep textbox trimmed and avoid overflow
            // when kiosk has been running for awhile

            Int32 maxsize = 1024000;
            Int32 dropsize = maxsize / 4;

            if (richtextbox.Text.Length > maxsize)
            {
                // this method preserves the text colouring
                // find the first end-of-line past the endmarker

                Int32 endmarker = richtextbox.Text.IndexOf('\n', dropsize) + 1;
                if (endmarker < dropsize)
                    endmarker = dropsize;

                richtextbox.Select(0, endmarker);
                richtextbox.Cut();
            }

            try
            {
                // trap exception which occurs when processing
                // as application shutdown is occurring

                richtextbox.SelectionStart = richtextbox.Text.Length;
                richtextbox.SelectionLength = 0;
                richtextbox.SelectionColor = textcolor;
                richtextbox.AppendText(DateTime.Now.ToString("HH:mm:ss.mmm") + " " + text);
            }
            catch (Exception ex)
            {
            }
        }
    }
}




//When calling this method, the URI should be specified as https://<NAMESPACE>.servicebus.windows.net/<EVENT_HUB_NAME>/publishers/<PUBLISHER_NAME>.
//For all tokens, the URI is identical, with the exception of PUBLISHER_NAME, which should be different for each token. Ideally, PUBLISHER_NAME represents the ID of the client that receives that token.

//<appSettings>
//  <!-- NotificationHubs specific app setings for messaging connections -->
//  <add key="Microsoft.Azure.NotificationHubs.ConnectionString"
//       value ="Endpoint=sb://[your namespace].notificationhub.windows.net;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=[your secret]"/>
//</appSettings>



// PER CHIAMARE UN EVENT HUB - se specifico un publisher name
// https://<EVENT_HUB_NAMESPACE>.servicebus.windows.net/<EVENT_HUB_NAME>/publishers/<PUBLISHER_NAME>

// PER CHIAMARE UN EVENT HUB - se NON specifico un publisher name
// https://<EVENT_HUB_NAMESPACE>.servicebus.windows.net/<EVENT_HUB_NAME>


// PER CHIAMARE UN NOTIFICATION HUB
// https://<NOTIFICATION_HUB_NAMESPACE>.servicebus.windows.net/<NOTIFICATION_HUB_NAME>/messages?api-version=2017-04



//https://mynamespace.servicebus.windows.net/mynotificationhub/messages?api-version=2013-10&test




//string eventHubName = "StefamaNotificationHub";
//string eventHubNamespace = "stefama";

//string connStr = "Endpoint=sb://stefama.servicebus.windows.net/;SharedAccessKeyName=SENDER;SharedAccessKey=m2cM5ptlr0sqczCNKd4YQxX26qGtIXp8DQ0CyML8ZAI=";




//private async void button2_Click(object sender, EventArgs e)
//{
//    if (textBox4.Text == string.Empty)
//    {
//        DisplayMessage.Error(this, "You need to generated a SAS before");
//        return;
//    }

//    textBox8.Text = null;

//    //https://{NOTIFICATION_HUB_NAMESPACE}.servicebus.windows.net/{NOTIFICATION_HUB_NAME}/messages?api-version=2017-04" />    

//    if (urlTemplate.Contains(HUB_NAMESPACE))
//    {
//        urlTemplate = urlTemplate.Replace(HUB_NAMESPACE, hubNamespace);
//    }
//    else
//    {
//        DisplayMessage.Error(this, $"Placeholder {hubNamespace} not found in \"UrlTemplate\" configuration item");
//        return;
//    }

//    if (urlTemplate.Contains(HUB_NAME))
//    {
//        urlTemplate = urlTemplate.Replace(HUB_NAME, hubName);
//    }
//    else
//    {
//        DisplayMessage.Error(this, $"Placeholder {hubName} not found in \"UrlTemplate\" configuration item");
//        return;
//    }


//    Uri requestUri = new Uri(urlTemplate);
//    //HttpWebRequest req = WebRequest.Create(requestUri) as HttpWebRequest;

//    HttpWebRequest req = HttpWebRequest.Create(requestUri) as HttpWebRequest;
//    req.Method = HTTP_POST;


//    req.Headers["Authorization"] = textBox5.Text;
//    req.ContentType = "application/json";

//    byte[] byteArray = Encoding.UTF8.GetBytes(textBox7.Text);
//    req.ContentLength = byteArray.Length;

//    try
//    {
//        // Get the request stream.
//        using (Stream dataStream = await req.GetRequestStreamAsync())
//        {
//            // Write the data to the request stream.
//            await dataStream.WriteAsync(byteArray, 0, byteArray.Length);
//            dataStream.Close();
//        }
//    }
//    catch (Exception ex)
//    {
//        DisplayMessage.Error(this, ex.Message);
//    }

//    try
//    {
//        // WEB RESPONSE

//        Task<WebResponse> task = req.GetResponseAsync();

//        //using (HttpWebResponse res = req.GetResponse() as HttpWebResponse)
//        using (HttpWebResponse res = await task as HttpWebResponse)
//        {
//            int StatusCode = (int)res.StatusCode;
//            string StatusDescription = res.StatusDescription;

//            string CharacterSet = res.CharacterSet;
//            string ContentEncoding = res.ContentEncoding;
//            long ContentLength = res.ContentLength;
//            string ContentType = res.ContentType;

//            DateTime LastModified = res.LastModified;

//            string ServerResponse = null;
//            using (Stream dataStream = res.GetResponseStream())
//            {
//                // Read the server response.
//                ServerResponse = await ReadToEndAsync(CharacterSet, dataStream);

//                // Close the Stream object.
//                dataStream.Close();
//            }

//            StringBuilder sb = new StringBuilder();

//            sb.AppendLine($"POST {urlTemplate}");
//            sb.Append(Environment.NewLine);

//            foreach (string key in req.Headers.Keys)
//            {
//                sb.AppendLine($"{key}: {req.Headers[key]}");
//            }

//            sb.Append(Environment.NewLine);
//            sb.AppendLine($"Response: {ServerResponse}");

//            textBox8.Text = sb.ToString();

//            res.Close();
//        }
//    }
//    catch (Exception ex)
//    {
//        DisplayMessage.Error(this, ex.Message);
//    }
//}

