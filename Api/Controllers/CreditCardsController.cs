using System.Net;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Filters.Authorization.AgentExistingFilters;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Models.Payments.CreditCards;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Payments.CreditCards;
using HappyTravel.Edo.Api.Services.Payments.Payfort;
using HappyTravel.Edo.Common.Enums;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace HappyTravel.Edo.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/cards")]
    [Produces("application/json")]
    public class CreditCardsController : BaseController
    {
        public CreditCardsController(ICreditCardService cardService,
            IAgentContext agentContext,
            IPayfortSignatureService signatureService)
        {
            _cardService = cardService;
            _agentContext = agentContext;
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
        public async Task<IActionResult> Get()
        {
            var agent = await _agentContext.GetAgent();
            return Ok(await _cardService.Get(agent));
        }


        /// <summary>
        ///     Deletes credit card
        /// </summary>
        [HttpDelete("{cardId}")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [AgentRequired]
        public async Task<IActionResult> Delete(int cardId)
        {
            var agent = await _agentContext.GetAgent();
            var (_, isFailure, error) = await _cardService.Delete(cardId, agent);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }


        /// <summary>
        ///     Calculates signature from json model
        /// </summary>
        /// <returns>signature</returns>
        [HttpPost("signatures")]
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
        public IActionResult GetSettings() => Ok(_cardService.GetTokenizationSettings());


        private readonly ICreditCardService _cardService;
        private readonly IAgentContext _agentContext;
        private readonly IPayfortSignatureService _signatureService;
    }
}