using System.Threading.Tasks;

namespace SecurityTest.Rest.Login
{
    using Microsoft.AspNetCore.Mvc;

    [Produces("application/json")]
    [Route("api/Profile")]
    public class LoginController : Controller
    {
        private readonly ISecurityTokenService securityTokenService;

        public LoginController(
            ISecurityTokenService securityTokenService)
        {
            this.securityTokenService = securityTokenService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]UserLogin userLogin)
        {
            await this.securityTokenService.AssignToken(this.HttpContext, userLogin);
            
            return this.Ok(new {
                Success = true
            });
        }
    }
}