using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace SecurityTest.Rest
{
    public interface ISecurityConfiguration
    {
        CookieBuilder CookieBuilder { get; }

        TokenValidationParameters TokenValidationParameters { get; }
    }

    public class SecurityConfiguration : ISecurityConfiguration
    {
        public SecurityConfiguration(
            CookieBuilder cookieBuilder, 
            TokenValidationParameters tokenValidationParameters)
        {
            CookieBuilder = cookieBuilder;
            TokenValidationParameters = tokenValidationParameters;
        }

        public CookieBuilder CookieBuilder { get; }
        public TokenValidationParameters TokenValidationParameters { get; }
    }
}