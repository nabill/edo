using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Filters.Authorization.InAgencyPermissionFilters;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Services.Files;
using HappyTravel.Edo.Common.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers.AgentControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/agencies")]
    [Produces("application/json")]
    public class AgencyImagesController : BaseController
    {
        public AgencyImagesController(IImageFileService imageFileService)
        {
            _imageFileService = imageFileService;
        }


        /// <summary>
        ///     Uploads an image
        /// </summary>
        [HttpPut("images")]
        [ProducesResponseType((int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [InAgencyPermissions(InAgencyPermissions.AgencyImagesManagement)]
        public async Task<IActionResult> Add([FromForm] IFormFile file) => OkOrBadRequest(await _imageFileService.Add(file));


        /// <summary>
        ///     Gets all uploaded images for the current agency
        /// </summary>
        [HttpGet("images")]
        [ProducesResponseType(typeof(List<SlimUploadedImage>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [InAgencyPermissions(InAgencyPermissions.AgencyImagesManagement)]
        public async Task<IActionResult> Get() => Ok(await _imageFileService.GetImages());


        /// <summary>
        ///     Deletes the image
        /// </summary>
        [HttpDelete("images/{fileName}")]
        [ProducesResponseType((int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [InAgencyPermissions(InAgencyPermissions.AgencyImagesManagement)]
        public async Task<IActionResult> Delete([FromRoute] string fileName) => OkOrBadRequest(await _imageFileService.Delete(fileName));


        private readonly IImageFileService _imageFileService;
    }
}
