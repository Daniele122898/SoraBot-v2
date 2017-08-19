using System;
using System.Text;

namespace SoraBot_v2.Extensions
{
    public static class StringEncoder
    {
        static readonly char[] padding = { '=' };

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes).TrimEnd(padding).Replace('+', '-').Replace('/', '_');
        }

        public static string Base64Decode(string base64EncodedData)
        {
            string incoming = base64EncodedData.Replace('_', '/').Replace('-', '+');
            switch (base64EncodedData.Length % 4)
            {
                case 2: incoming += "=="; break;
                case 3: incoming += "="; break;
            }
            var base64EncodedBytes = Convert.FromBase64String(incoming);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}