using System.Threading.Tasks;

namespace SecurityTest.Rest
{
    public interface IClientAuthenticationHandler
    {
        Task<bool> Authenticate(UserLogin login);
    }
}