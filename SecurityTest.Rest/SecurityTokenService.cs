using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using SecurityTest.Rest;

namespace SecurityTest.Rest
{
    public class SecurityTokenService : ISecurityTokenService
    {
        private readonly ISecurityConfiguration securityConfiguration;
        private readonly IClientAuthenticationHandler clientAuthenticationHandler;

        public SecurityTokenService(
            ISecurityConfiguration securityConfiguration,
            IClientAuthenticationHandler clientAuthenticationHandler)
        {
            this.securityConfiguration = securityConfiguration;
            this.clientAuthenticationHandler = clientAuthenticationHandler;
        }

        public async Task AssignToken(HttpContext context, UserLogin userLogin)
        {
            

            if (!await this.clientAuthenticationHandler.Authenticate(userLogin))
            {
                context.Response.StatusCode = 401;
                await context.WriteStringToResponse("Unauthorized");
                return;
            }

            var bearerToken = this.CreateToken(await this.clientAuthenticationHandler.GetClaims(userLogin));

            context.Response.StatusCode = 200;
            context.WriteCookieToResponse(this.securityConfiguration.CookieBuilder, bearerToken);
            await context.WriteObjectToResponse(new { success = true });
        }

        private string CreateToken(IEnumerable<Claim> claims)
        {
            var token = new JwtSecurityToken(
                this.securityConfiguration.TokenValidationParameters.ValidIssuer,
                this.securityConfiguration.TokenValidationParameters.ValidAudience,
                claims,
                null,
                DateTime.UtcNow.AddDays(1),
                new SigningCredentials(this.securityConfiguration.TokenValidationParameters.IssuerSigningKey, SecurityAlgorithms.RsaSha256));

            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();

            return jwtSecurityTokenHandler.WriteToken(token);
        }
    }
}