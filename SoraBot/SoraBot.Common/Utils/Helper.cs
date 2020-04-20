using System;

namespace SoraBot.Common.Utils
{
    public static class Helper
    {
        public static bool UrlValidUri(string url)
            => Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}