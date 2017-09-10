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
            return this.Ok(new {Name = "Callum"});
        }
    }
}