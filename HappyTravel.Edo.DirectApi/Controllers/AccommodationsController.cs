using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.DirectApi.Models.Static;
using HappyTravel.Edo.DirectApi.Services.Static;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.DirectApi.Controllers
{
    /// <summary>
    /// These endpoints provide the static data for accommodations. You can get a list of accommodations modified since a given date, 
    /// or you can get a single accommodation if you know the ID.
    /// </summary>
    [ApiController]
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/{version:apiVersion}/static/accommodations", Name = "Static Data", Order = 1)]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public class AccommodationsController : ControllerBase
    {
        public AccommodationsController(AccommodationService accommodationService)
        {
            _accommodationService = accommodationService;
        }


        /// <summary>
        /// Get accommodation list
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<Accommodation>>> GetAccommodationsList(DateTimeOffset? modified, [Range(1, 500)]int top = 100, int skip = 0)
        {
            var (isSuccess, _, accommodations, error) = await _accommodationService.GetAccommodationList(modified, top, skip, "en");

            return isSuccess
                ? accommodations
                : BadRequest(ProblemDetailsBuilder.Build(error));
        }


        /// <summary>
        /// Get an accommodation by ID
        /// </summary>
        [HttpGet("{accommodationId}")]
        public async Task<ActionResult<Accommodation>> GetAccommodation(string accommodationId)
        {
            var (isSuccess, _, accommodation, error) = await _accommodationService.GetAccommodationById(accommodationId, "en");
            
            return isSuccess
                ? accommodation
                : BadRequest(ProblemDetailsBuilder.Build(error));
        }


        private readonly AccommodationService _accommodationService;
    }
}