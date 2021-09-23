using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.AdministratorServices.Models.Mapper;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.MapperContracts.Public.Accommodations.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.AdministratorServices.AccommodationManagementServices
{
    public interface IMapperManagementClient
    {
        Task<Result<Unit, ProblemDetails>> CombineAccommodations(string baseHtAccommodationId, string combinedHtAccommodationId, CancellationToken cancellationToken);

        Task<Result<Unit, ProblemDetails>> DeactivateAccommodations(DeactivateAccommodationsRequest request, AccommodationDeactivationReasons deactivationReason, CancellationToken cancellationToken);

        Task<Result<Unit, ProblemDetails>> RemoveSupplier(string htAccommodationId, RemoveSupplierRequest request, CancellationToken cancellationToken);
    }
}