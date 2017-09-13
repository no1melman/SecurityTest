using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SecurityTest.Rest
{
    public interface ISecurityTokenService
    {
        Task AssignToken(HttpContext context, UserLogin userLogin);
    }
}