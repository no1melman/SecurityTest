using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SecurityTest.Rest
{
    public class ClientAuthenticationServerMiddleware
    {
        private readonly ISecurityTokenService securityTokenService;

        public ClientAuthenticationServerMiddleware(RequestDelegate next, ISecurityTokenService securityTokenService)
        {
            this.securityTokenService = securityTokenService;
        }

        public async Task Invoke(HttpContext context)
        {
            var userLogin = await context.DeserialiseRequestBodyAsync<UserLogin>();

            await this.securityTokenService.AssignToken(context, userLogin);
        }
    }
}