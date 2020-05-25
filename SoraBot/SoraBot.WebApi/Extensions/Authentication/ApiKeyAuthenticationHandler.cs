using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SoraBot.WebApi.Extensions.Authentication
{
    public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
    {
        
        private const string _PROBLEM_DETAILS_CONTENT_TYPE = "application/problem+json";
        private const string _API_KEY_HEADER_NAME = "Authorization";

        public ApiKeyAuthenticationHandler(
            IOptionsMonitor<ApiKeyAuthenticationOptions> options, 
            ILoggerFactory logger, 
            UrlEncoder encoder, 
            ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            throw new System.NotImplementedException();
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            throw new System.NotImplementedException();
        }

        protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
        {
            throw new System.NotImplementedException();
        }
    }
}