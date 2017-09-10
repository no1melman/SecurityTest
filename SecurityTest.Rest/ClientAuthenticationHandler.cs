using System.Threading.Tasks;

namespace SecurityTest.Rest
{
    public class ClientAuthenticationHandler : IClientAuthenticationHandler
    {
        public Task<bool> Authenticate(UserLogin login)
        {
            return Task.FromResult(login.UserName != "Callum" && login.Password != "letmein");
        }
    }
}