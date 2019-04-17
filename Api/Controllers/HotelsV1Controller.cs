using System;
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
        public IActionResult Get()
        {
            throw new ArgumentNullException();
        }
    }
}
