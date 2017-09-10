using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SecurityTest.Rest
{
    public class TokenExtractionMiddleware
    {
        private const string AuthorisationHeader = "Authorization";

        private readonly RequestDelegate next;
        private readonly CookieBuilder cookieBuilder;

        public TokenExtractionMiddleware(RequestDelegate next, CookieBuilder cookieBuilder)
        {
            this.next = next;
            this.cookieBuilder = cookieBuilder;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Cookies.Count <= 0)
            {
                await this.next(context);

                return;
            }

            var authCookie = context.Request.Cookies.FirstOrDefault(x => x.Key == this.cookieBuilder.Name);

            if (!authCookie.Equals(default(KeyValuePair<string, string>)))
            {
                context.Request.Headers.Add(AuthorisationHeader, $"Bearer {authCookie.Value}");
            }

            await this.next(context);
        }
    }
}