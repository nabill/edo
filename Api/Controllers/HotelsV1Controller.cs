using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/hotels")]
    [Produces("application/json")]
    public class HotelsV1Controller : ControllerBase
    {
        [HttpGet]
        [Authorize]
        public IActionResult Get()
        {
            var claims = User.Claims.Select(c => $"{c.Type}: {c.Value}");
            return Ok(claims);
        }
    }
}
