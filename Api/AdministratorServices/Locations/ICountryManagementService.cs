using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Locations;
using Microsoft.AspNetCore.Mvc;

namespace Api.AdministratorServices.Locations
{
    public interface ICountryManagementService
    {
        Task<List<Country>> Get(CancellationToken cancellationToken = default);

        Task<Result<List<Country>, ProblemDetails>> Actualize(string language = LocalizationHelper.DefaultLanguageCode, CancellationToken cancellationToken = default);
    }
}