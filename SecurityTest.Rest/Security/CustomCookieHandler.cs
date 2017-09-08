using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SecurityTest.Rest.Security
{
    public class CustomCookieHandler : CookieAuthenticationHandler
    {
        public CustomCookieHandler(IOptionsMonitor<CookieAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var authenticationTicket = new AuthenticationTicket(new ClaimsPrincipal(), this.Scheme.ToString());

            var authenticateResult = AuthenticateResult.Success(authenticationTicket);

            return Task.FromResult(authenticateResult);
        }
    }
}