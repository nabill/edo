using System.Net;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Filters.Authorization.AgentExistingFilters;
using HappyTravel.Edo.Api.Filters.Authorization.AgencyVerificationStatesFilters;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Models.Payments.CreditCards;
using HappyTravel.Edo.Api.Services.Agents;
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
            IAgentContextService agentContextService,
            IPayfortSignatureService signatureService)
        {
            _cardsManagementService = cardsManagementService;
            _agentContextService = agentContextService;
            _signatureService = signatureService;
        }


        /// <summary>
        ///     Returns cards, available for current agent/counterparty
        /// </summary>
        /// <returns>List of cards.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(CreditCardInfo[]), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [AgentRequired]
        [MinAgencyVerificationState(AgencyVerificationStates.FullAccess)]
        public async Task<IActionResult> Get()
        {
            var agent = await _agentContextService.GetAgent();
            return Ok(await _cardsManagementService.Get(agent));
        }


        /// <summary>
        ///     Deletes credit card
        /// </summary>
        [HttpDelete("{cardId}")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [AgentRequired]
        [MinAgencyVerificationState(AgencyVerificationStates.FullAccess)]
        public async Task<IActionResult> Delete(int cardId)
        {
            var agent = await _agentContextService.GetAgent();
            var (_, isFailure, error) = await _cardsManagementService.Delete(cardId, agent);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
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
        private readonly IAgentContextService _agentContextService;
        private readonly IPayfortSignatureService _signatureService;
    }
}