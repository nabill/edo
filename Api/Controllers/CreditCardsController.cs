using System;
using System.Net;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Payments.CreditCard;
using HappyTravel.Edo.Api.Services.Customers;
using HappyTravel.Edo.Api.Services.Payments;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Common.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace HappyTravel.Edo.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/cards")]
    [Produces("application/json")]
    public class CreditCardsController : BaseController
    {
        public CreditCardsController(ICreditCardService cardService, ICustomerContext customerContext, IPayfortSignatureService signatureService,
            IOptions<PayfortOptions> options)
        {
            _cardService = cardService;
            _customerContext = customerContext;
            _signatureService = signatureService;
            _options = options.Value;
        }

        /// <summary>
        ///     Returns available cards
        /// </summary>
        /// <returns>List of cards.</returns>
        [HttpGet()]
        [ProducesResponseType(typeof(CreditCardInfo[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Get()
        {
            var (_, companyFailure, company, companyError) = await _customerContext.GetCompany();
            if (companyFailure)
                return BadRequest(ProblemDetailsBuilder.Build(companyError));

            var (_, customerFailure, customer, customerError) = await _customerContext.GetCustomer();
            if (customerFailure)
                return BadRequest(ProblemDetailsBuilder.Build(customerError));
            
            return Ok(await _cardService.Get(customer, company));
        }

        /// <summary>
        ///     Save credit card
        /// </summary>
        /// <returns>Saved credit card info</returns>
        [HttpPost()]
        [ProducesResponseType(typeof(CreditCardInfo), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Create(SaveCreditCardRequest request)
        {
            int ownerId;
            switch (request.OwnerType)
            {
                case CreditCardOwnerType.Company:
                    var (_, companyFailure, company, companyError) = await _customerContext.GetCompany();
                    if (companyFailure)
                        return BadRequest(ProblemDetailsBuilder.Build(companyError));

                    ownerId = company.Id;
                    break;
                case CreditCardOwnerType.Customer:
                    var (_, customerFailure, customer, customerError) = await _customerContext.GetCustomer();
                    if (customerFailure)
                        return BadRequest(ProblemDetailsBuilder.Build(customerError));
                    ownerId = customer.Id;
                    break;
                default: throw new NotImplementedException();
            }

            return OkOrBadRequest(await _cardService.Save(request, ownerId));
        }

        /// <summary>
        ///     Delete credit card
        /// </summary>
        /// <returns>204</returns>
        [HttpDelete("{cardId}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Delete(int cardId)
        {
            var (_, companyFailure, company, companyError) = await _customerContext.GetCompany();
            if (companyFailure)
                return BadRequest(ProblemDetailsBuilder.Build(companyError));

            var (_, customerFailure, customer, customerError) = await _customerContext.GetCustomer();
            if (customerFailure)
                return BadRequest(ProblemDetailsBuilder.Build(customerError));

            var (_, isFailure, error) = await _cardService.Delete(cardId, customer, company);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }

        /// <summary>
        ///     Calculate signature from json model
        /// </summary>
        /// <returns>signature</returns>
        [HttpPost("signature/{type}")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        public IActionResult Signature([FromBody]JObject value, SignatureTypes type)
        {
            if (!Enum.IsDefined(typeof(SignatureTypes), type) || type == SignatureTypes.Unknown)
            {
                return BadRequest(ProblemDetailsBuilder.Build("Invalid signature type"));
            }
            var signature = _signatureService.Calculate(value, type == SignatureTypes.Request ? _options.ShaRequestPhrase : _options.ShaResponsePhrase);
            return Ok(signature);
        }

        /// <summary>
        ///     Get settings for tokenization
        /// </summary>
        /// <returns>Settings for tokenization</returns>
        [ProducesResponseType(typeof(TokenizationSettings), (int)HttpStatusCode.OK)]
        [HttpGet("settings")]
        public IActionResult GetSettings()
        {
            return Ok(new TokenizationSettings(_options.AccessCode, _options.Identifier, _options.TokenizationUrl));
        }

        private readonly ICreditCardService _cardService;
        private readonly ICustomerContext _customerContext;
        private readonly IPayfortSignatureService _signatureService;
        private readonly PayfortOptions _options;
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings()
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            }
        };
    }
}
