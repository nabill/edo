using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Management.Enums;
using HappyTravel.Edo.Api.Services.Files;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CSharpFunctionalExtensions;

namespace HappyTravel.Edo.Api.Controllers.AdministratorControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/admin")]
    [Produces("application/json")]
    public class ContractFileController : BaseController
    {


        public ContractFileController(IContractFileService contractFileService)
        {
            _contractFileService = contractFileService;
        }

        /// <summary>
        /// Uploads contract pdf file
        /// </summary>
        /// <param name="counterpartyId">Id of the counterparty to save the contract file for</param>
        /// <param name="file">A pdf file of the contract with the given counterparty</param>
        [HttpPut("conterparties/{counterpartyId}/contract-file")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.CounterpartyManagement)]
        public async Task<IActionResult> SetSystemSettings(int counterpartyId, [FromForm] IFormFile file)
        {
            var (_, isFailure, error) = await _contractFileService.Save(counterpartyId, file);

            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok();
        }


        private readonly IContractFileService _contractFileService;
    }
}
