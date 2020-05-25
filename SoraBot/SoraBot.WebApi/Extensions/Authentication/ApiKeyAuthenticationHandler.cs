using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoraBot.Data.Configurations;

namespace SoraBot.WebApi.Extensions.Authentication
{
    public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
    {
        private const string _PROBLEM_DETAILS_CONTENT_TYPE = "application/problem+json";
        private const string _API_KEY_HEADER_NAME = "Authorization";

        private ApiConfig _config;

        public ApiKeyAuthenticationHandler(
            IOptionsMonitor<ApiKeyAuthenticationOptions> options, 
            ILoggerFactory logger, 
            UrlEncoder encoder, 
            ISystemClock clock,
            IOptions<ApiConfig> config) : base(options, logger, encoder, clock)
        {
            _config = config.Value;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Check if the request has a Authorization header
            if (!this.Request.Headers.TryGetValue(_API_KEY_HEADER_NAME, out var apiKeyHeaderValues) || apiKeyHeaderValues.Count == 0)
                return Task.FromResult(AuthenticateResult.NoResult());

            // Check if header has value
            var keyHeader = apiKeyHeaderValues[0];
            if (string.IsNullOrWhiteSpace(keyHeader))
                return Task.FromResult(AuthenticateResult.NoResult());
            
            // Check if correct key
            if (keyHeader != _config.ApiToken)
                return Task.FromResult(AuthenticateResult.Fail("Invalid API key provided"));
            
            // Proper key so let him pass
            // Here is were you'd normally add the claims like userid or username that can be used in authorized methods
            // but here we don't have anything connected to the api key. So we just pass empty values.
            var identity = new ClaimsIdentity(Options.AuthenticationType);
            var principal = new ClaimsPrincipal(identity); 
            var ticket = new AuthenticationTicket(principal, Options.Scheme);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = 401; // Unauthorized
            Response.ContentType = _PROBLEM_DETAILS_CONTENT_TYPE;
            var problemDetails = new UnauthorizedResult();

            await Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
        }

        protected override async Task HandleForbiddenAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = 403; // Forbidden
            Response.ContentType = _PROBLEM_DETAILS_CONTENT_TYPE;
            var problemDetails = new ForbidResult();

            await Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
        }
    }
}