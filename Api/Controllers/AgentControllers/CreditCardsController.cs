using System.Net;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Services.Payments.CreditCards;
using HappyTravel.Edo.Api.Services.Payments.Payfort;
using HappyTravel.Edo.Common.Enums;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace HappyTravel.Edo.Api.Controllers.AgentControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/cards")]
    [Produces("application/json")]
    public class CreditCardsController : BaseController
    {
        public CreditCardsController(ICreditCardsManagementService cardsManagementService,
            IPayfortSignatureService signatureService)
        {
            _cardsManagementService = cardsManagementService;
            _signatureService = signatureService;
        }


        /// <summary>
        ///     Calculates signature from json model
        /// </summary>
        /// <returns>signature</returns>
        [HttpPost("signatures/calculate")]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        public IActionResult CalculateSignature([FromBody] JObject value)
        {
            var (_, isFailure, signature, error) = _signatureService.Calculate(value, SignatureTypes.Request);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok(signature);
        }


        /// <summary>
        ///     Gets settings for tokenization
        /// </summary>
        /// <returns>Settings for tokenization</returns>
        [ProducesResponseType(typeof(TokenizationSettings), (int) HttpStatusCode.OK)]
        [HttpGet("settings")]
        public IActionResult GetSettings() => Ok(_cardsManagementService.GetTokenizationSettings());


        private readonly ICreditCardsManagementService _cardsManagementService;
        private readonly IPayfortSignatureService _signatureService;
    }
}