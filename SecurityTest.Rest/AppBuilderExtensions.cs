using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace SecurityTest.Rest
{
    public static class AppBuilderExtensions
    {
        public static void UseTokenExtraction(this IApplicationBuilder app, CookieBuilder cookieBuilder)
        {
            app.UseMiddleware<TokenExtractionMiddleware>(cookieBuilder);
        }

        public static void UseAuthenticateAllRequests(this IApplicationBuilder app)
        {
            app.UseMiddleware<AuthenticateAllRequestsMiddleware>();
        }

        public static void UseClientAuthenticationServer(this IApplicationBuilder app)
        {
            app.UseMiddleware<ClientAuthenticationServerMiddleware>();
        }
    }
}