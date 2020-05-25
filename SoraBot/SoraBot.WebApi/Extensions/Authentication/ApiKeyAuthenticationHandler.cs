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
            throw new System.NotImplementedException();
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