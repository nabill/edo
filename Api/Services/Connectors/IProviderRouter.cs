using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.DataProviders;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.EdoContracts.Accommodations;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Services.Connectors
{
    public interface IProviderRouter
    {
        Task<Result<CombinedAvailabilityDetails>> GetAvailability(List<DataProviders> dataProviders, AvailabilityRequest availabilityRequest,
            string languageCode);


        Task<Result<SingleAccommodationAvailabilityDetails, ProblemDetails>> GetAvailable(DataProviders dataProvider, string accommodationId,
            string availabilityId, string languageCode);


        Task<Result<SingleAccommodationAvailabilityDetailsWithDeadline?, ProblemDetails>> GetExactAvailability(DataProviders dataProvider, string availabilityId,
            Guid roomContractSetId, string languageCode);


        Task<Result<AccommodationDetails, ProblemDetails>> GetAccommodation(DataProviders dataProvider, string id, string languageCode);

        Task<Result<BookingDetails, ProblemDetails>> Book(DataProviders dataProvider, BookingRequest request, string languageCode);

        Task<Result<VoidObject, ProblemDetails>> CancelBooking(DataProviders dataProvider, string referenceCode);

        Task<Result<DeadlineDetails,ProblemDetails>> GetDeadline(DataProviders dataProvider, string availabilityId, Guid roomContractSetId, string languageCode);


        Task<Result<BookingDetails, ProblemDetails>> GetBookingDetails(DataProviders dataProvider, string referenceCode,
            string languageCode);
    }
}