using System.Net;
using HappyTravel.Edo.Api.Services.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/[controller]")]
    [Produces("application/json")]
    public class VersionsController : ControllerBase
    {
        public VersionsController(IVersionService service)
        {
            _service = service;
        }


        /// <summary>
        /// Returns a current build version of the system.
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        public IActionResult Get()
        {
            return Ok(_service.Get());
        }
    
        
        private readonly IVersionService _service;
    }
}
