using System;
using System.Net;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Payments.CreditCard;
using HappyTravel.Edo.Api.Services.Customers;
using HappyTravel.Edo.Api.Services.Payments;
using HappyTravel.Edo.Common.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/cards")]
    [Produces("application/json")]
    public class CreditCardsController : BaseController
    {
        public CreditCardsController(ICreditCardService cardService, ICustomerContext customerContext)
        {
            _cardService = cardService;
            _customerContext = customerContext;
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
        ///     Create new credit card
        /// </summary>
        /// <returns>Created credit card info</returns>
        [HttpPost()]
        [ProducesResponseType(typeof(CreditCardInfo), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Create(CreateCreditCardRequest request)
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

            return OkOrBadRequest(await _cardService.Create(request, ownerId, LanguageCode));
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

        private readonly ICreditCardService _cardService;
        private readonly ICustomerContext _customerContext;
    }
}
