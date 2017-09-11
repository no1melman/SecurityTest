using System.Security.Claims;
using System.Threading.Tasks;

namespace SecurityTest.Rest
{
    public class ClientAuthenticationHandler : IClientAuthenticationHandler
    {
        public Task<bool> Authenticate(UserLogin login)
        {
            return Task.FromResult(login.UserName == "Callum" && login.Password == "letmein");
        }

        public Task<Claim[]> GetClaims(UserLogin login)
        {
            return Task.FromResult(new[]
            {
                new Claim("id", "1"),
                new Claim("name", "Callum")
            });
        }
    }
}