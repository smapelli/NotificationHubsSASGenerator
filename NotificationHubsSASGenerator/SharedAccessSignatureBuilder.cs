using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NotificationHubsSASGenerator
{
    internal static class SharedAccessSignatureBuilder
    {
        public static string GetSharedAccessSignature(string keyName, string sharedAccessKey, string resource, TimeSpan tokenTimeToLive)
        {
            if (string.IsNullOrWhiteSpace(keyName))
            {
                throw new ArgumentException("keyName");
            }
            if (string.IsNullOrWhiteSpace(sharedAccessKey))
            {
                throw new ArgumentException("sharedAccessKey");
            }
            if (string.IsNullOrWhiteSpace(resource))
            {
                throw new ArgumentException("resource");
            }
            if (tokenTimeToLive < TimeSpan.Zero)
            {
                throw new ArgumentException("tokenTimeToLive");
            }
            byte[] bytes = Encoding.UTF8.GetBytes(sharedAccessKey);
            return BuildSignature(keyName, bytes, resource, tokenTimeToLive);
        }

        private static readonly DateTime EpochTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        private static string BuildSignature(string keyName, byte[] encodedSharedAccessKey, string targetUri, TimeSpan timeToLive)
        {
            string text = BuildExpiresOn(timeToLive);
            string text2 = HttpUtility.UrlEncode(targetUri.ToLowerInvariant());

            List<string> list = new List<string>();
            list.Add(text2);
            list.Add(text);

            string str = Sign(string.Join("\n", list), encodedSharedAccessKey);
            return string.Format(CultureInfo.InvariantCulture, "{0} {1}={2}&{3}={4}&{5}={6}&{7}={8}", "SharedAccessSignature", "sr", text2, "sig", HttpUtility.UrlEncode(str), "se", HttpUtility.UrlEncode(text), "skn", HttpUtility.UrlEncode(keyName));
        }

        private static string BuildExpiresOn(TimeSpan timeToLive)
        {
            return Convert.ToString(Convert.ToInt64(DateTime.UtcNow.Add(timeToLive).Subtract(EpochTime).TotalSeconds, CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
        }

        private static string Sign(string requestString, byte[] encodedSharedAccessKey)
        {
            using (HMACSHA256 hMACSHA = new HMACSHA256(encodedSharedAccessKey))
            {
                return Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(requestString)));
            }
        }
    }
}
