using System.Collections.Generic;
using System.Linq;

namespace SecurityTest.Rest.Profile
{
    using Microsoft.AspNetCore.Mvc;

    [Produces("application/json")]
    [Route("api/Profile")]
    public class ProfileController : Controller
    {
        [HttpGet]
        public IActionResult Get()
        {
            return this.Ok(new {Claims = this.HttpContext.User.Claims.ToDictionary(x => x.Subject.ToString(), x => x.Value) });
        }
    }
}