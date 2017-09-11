using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace SecurityTest.Rest
{
    public class ClientAuthenticationServerMiddleware
    {
        private readonly CookieBuilder cookieBuilder;
        private readonly TokenValidationParameters tokenValidationParameters;
        private readonly IClientAuthenticationHandler clientAuthenticationHandler;

        public ClientAuthenticationServerMiddleware(RequestDelegate next, CookieBuilder cookieBuilder, TokenValidationParameters tokenValidationParameters, IClientAuthenticationHandler clientAuthenticationHandler)
        {
            this.cookieBuilder = cookieBuilder;
            this.tokenValidationParameters = tokenValidationParameters;
            this.clientAuthenticationHandler = clientAuthenticationHandler;
        }

        public async Task Invoke(HttpContext context)
        {
            var userLogin = context.DeserialiseRequestBody<UserLogin>();

            if (!await this.clientAuthenticationHandler.Authenticate(userLogin))
            {
                context.Response.StatusCode = 401;
                await context.WriteStringToResponse("Unauthorized");
                return;
            }

            var bearerToken = this.CreateToken(await this.clientAuthenticationHandler.GetClaims(userLogin));

            context.Response.StatusCode = 200;
            context.WriteCookieToResponse(this.cookieBuilder, bearerToken);
            await context.WriteObjectToResponse(new { success = true });
        }

        private string CreateToken(IEnumerable<Claim> claims)
        {
            var token = new JwtSecurityToken(
                this.tokenValidationParameters.ValidIssuer,
                this.tokenValidationParameters.ValidAudience,
                claims,
                null,
                DateTime.UtcNow.AddDays(1),
                new SigningCredentials(this.tokenValidationParameters.IssuerSigningKey, SecurityAlgorithms.RsaSha256));

            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();

            return jwtSecurityTokenHandler.WriteToken(token);
        }
    }
}