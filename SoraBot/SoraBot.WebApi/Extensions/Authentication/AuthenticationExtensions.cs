using System;
using Microsoft.AspNetCore.Authentication;

namespace SoraBot.WebApi.Extensions.Authentication
{
    public static class AuthenticationExtensions
    {
        public static AuthenticationBuilder AddApiKeySupport(this AuthenticationBuilder ab,
            Action<ApiKeyAuthenticationOptions> options)
        {
            return ab.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationOptions.DEFAULT_SCHEME, options);
        }
    }
}