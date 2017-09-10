using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace SecurityTest.Rest
{
    public class AuthenticateAllRequestsMiddleware
    {
        private readonly RequestDelegate next;

        public AuthenticateAllRequestsMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var user = context.User; // We can do this because of  options.AutomaticAuthenticate = true;
            if (user?.Identity?.IsAuthenticated ?? false)
            {
                await this.next(context);
            }
            else
            {
                // We can do this because of options.AutomaticChallenge = true;
                await context.ChallengeAsync();
            }
        }
    }
}