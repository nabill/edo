using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.AdministratorServices.Models.Mapper;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.MapperContracts.Public.Locations;
using HappyTravel.MapperContracts.Public.Management.Accommodations.ManualCorrections;
using HappyTravel.MapperContracts.Public.Management.Accommodations.SlimAccommodations;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.AdministratorServices.Mapper.AccommodationManagementServices
{
    public interface IMapperManagementClient
    {
        Task<Result<Unit, ProblemDetails>> MergeAccommodations(AccommodationsMergeRequest accommodationsMergeRequest, CancellationToken cancellationToken);
        Task<Result<Unit, ProblemDetails>> DeactivateAccommodationManually(string htAccommodationId, string deactivationReasonDescription, CancellationToken cancellationToken);
        Task<Result<Unit, ProblemDetails>> ActivateAccommodationManually(string htAccommodationId, CancellationToken cancellationToken);
        Task<Result<Unit, ProblemDetails>> RemoveSupplier(string htAccommodationId, RemoveSupplierRequest request, CancellationToken cancellationToken);
        Task<Result<HappyTravel.MapperContracts.Public.Management.Accommodations.DetailedAccommodations.DetailedAccommodation, ProblemDetails>> GetDetailedAccommodationData(string accommodationHtId, string languageCode, CancellationToken cancellationToken);
        Task<Result<SlimAccommodationDataResponse, ProblemDetails>> SearchAccommodations(AccommodationSearchRequest request, CancellationToken cancellationToken);
        Task<Result<Dictionary<int, string>, ProblemDetails>> GetDeactivationReasonTypes(CancellationToken cancellationToken);
        Task<Result<Dictionary<int, string>, ProblemDetails>> GetRatingTypes(CancellationToken cancellationToken);
        Task<Result<List<SlimCountry>, ProblemDetails>> GetAllCountries(string languageCode, CancellationToken cancellationToken);
        Task<Result<List<CountryData>, ProblemDetails>> SearchCountries(string query, string languageCode, CancellationToken cancellationToken);
        Task<Result<List<LocalityData>, ProblemDetails>> SearchLocalities(int countryId, string query, string languageCode, CancellationToken cancellationToken);
        Task<Result<Unit, ProblemDetails>> AddManualCorrectionData(string htAccommodationId, AccommodationManualCorrectionRequest accommodationManualCorrectionRequest, CancellationToken cancellationToken);
    }
}