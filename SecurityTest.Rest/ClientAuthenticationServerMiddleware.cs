using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace SecurityTest.Rest
{
    public class ClientAuthenticationServerMiddleware
    {
        private readonly RequestDelegate next;
        private readonly CookieBuilder cookieBuilder;
        private readonly TokenValidationParameters tokenValidationParameters;
        private readonly IClientAuthenticationHandler clientAuthenticationHandler;

        public ClientAuthenticationServerMiddleware(RequestDelegate next, CookieBuilder cookieBuilder, TokenValidationParameters tokenValidationParameters, IClientAuthenticationHandler clientAuthenticationHandler)
        {
            this.next = next;
            this.cookieBuilder = cookieBuilder;
            this.tokenValidationParameters = tokenValidationParameters;
            this.clientAuthenticationHandler = clientAuthenticationHandler;
        }

        public async Task Invoke(HttpContext context)
        {
            var userLogin = context.DeserialiseRequestBody<UserLogin>();

            if (await this.clientAuthenticationHandler.Authenticate(userLogin))
            {
                context.Response.StatusCode = 401;
                await context.WriteStringToResponse("Unauthorized");
                return;
            }

            var bearerToken = this.CreateToken();

            context.Response.StatusCode = 200;
            context.WriteCookieToResponse(this.cookieBuilder, bearerToken);
            await context.WriteObjectToResponse(new { success = true });
            return;
        }

        private string CreateToken()
        {
            var token = new JwtSecurityToken(
                this.tokenValidationParameters.ValidIssuer,
                this.tokenValidationParameters.ValidAudience,
                new ClaimsIdentity().Claims,
                null,
                DateTime.UtcNow.AddDays(1),
                new SigningCredentials(this.tokenValidationParameters.IssuerSigningKey, SecurityAlgorithms.RsaSha256));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}