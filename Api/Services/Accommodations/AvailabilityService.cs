using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Markups.Availability;
using HappyTravel.Edo.Api.Services.Connectors;
using HappyTravel.Edo.Api.Services.Customers;
using HappyTravel.Edo.Api.Services.Locations;
using HappyTravel.Edo.Api.Services.Markups.Availability;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Internals;
using HappyTravel.EdoContracts.GeoData;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Services.Accommodations
{
    public class AvailabilityService : IAvailabilityService
    {
        public AvailabilityService(ILocationService locationService,
            ICustomerContext customerContext,
            IAvailabilityMarkupService markupService,
            IAvailabilityResultsCache availabilityResultsCache,
            IProviderRouter providerRouter,
            IDeadlineDetailsCache deadlineDetailsCache)
        {
            _locationService = locationService;
            _customerContext = customerContext;
            _markupService = markupService;
            _availabilityResultsCache = availabilityResultsCache;
            _providerRouter = providerRouter;
            _deadlineDetailsCache = deadlineDetailsCache;
        }


        public async ValueTask<Result<CombinedAvailabilityDetails, ProblemDetails>> GetAvailable(Models.Availabilities.AvailabilityRequest request,
            string languageCode)
        {
            var (_, isFailure, location, error) = await _locationService.Get(request.Location, languageCode);
            if (isFailure)
                return Result.Fail<CombinedAvailabilityDetails, ProblemDetails>(error);

            return await ExecuteRequest()
                .OnSuccess(ApplyMarkup)
                .OnSuccess(ReturnResponseWithMarkup);


            async Task<Result<CombinedAvailabilityDetails, ProblemDetails>> ExecuteRequest()
            {
                var roomDetails = request.RoomDetails
                    .Select(r => new RoomRequestDetails(r.AdultsNumber, r.ChildrenNumber, r.ChildrenAges, r.Type,
                        r.IsExtraBedNeeded))
                    .ToList();

                var contract = new AvailabilityRequest(request.Nationality, request.Residency, request.CheckInDate,
                    request.CheckOutDate,
                    request.Filters, roomDetails, new Location(location.Name, location.Locality, location.Country, location.Coordinates, location.Distance, location.Source, location.Type),
                    request.PropertyType, request.Ratings);

                var (isSuccess, _, details, providerError) = await _providerRouter.GetAvailability(location.DataProviders, contract, languageCode);
                return isSuccess
                    ? Result.Ok<CombinedAvailabilityDetails, ProblemDetails>(details)
                    : ProblemDetailsBuilder.Fail<CombinedAvailabilityDetails>(providerError);
            }


            async Task<AvailabilityDetailsWithMarkup> ApplyMarkup(CombinedAvailabilityDetails response)
            {
                var customer = await _customerContext.GetCustomer();
                return await _markupService.Apply(customer, response);
            }


            CombinedAvailabilityDetails ReturnResponseWithMarkup(AvailabilityDetailsWithMarkup markup) => markup.ResultResponse;
        }


        public async Task<Result<ProviderData<SingleAccommodationAvailabilityDetails>, ProblemDetails>> GetAvailable(DataProviders dataProvider,
            string accommodationId, long availabilityId,
            string languageCode)
        {
            return await ExecuteRequest()
                .OnSuccess(ApplyMarkup)
                .OnSuccess(ReturnResponseWithMarkup)
                .OnSuccess(AddProviderData);


            Task<Result<SingleAccommodationAvailabilityDetails, ProblemDetails>> ExecuteRequest()
            {
                return _providerRouter.GetAvailable(dataProvider, accommodationId, availabilityId, languageCode);
            }


            async Task<SingleAccommodationAvailabilityDetailsWithMarkup> ApplyMarkup(SingleAccommodationAvailabilityDetails response)
            {
                var customer = await _customerContext.GetCustomer();
                return await _markupService.Apply(customer, response);
            }


            SingleAccommodationAvailabilityDetails ReturnResponseWithMarkup(SingleAccommodationAvailabilityDetailsWithMarkup markup) => markup.ResultResponse;


            ProviderData<SingleAccommodationAvailabilityDetails> AddProviderData(SingleAccommodationAvailabilityDetails availabilityDetails)
                => ProviderData.Create(dataProvider, availabilityDetails);
        }


        public async Task<Result<ProviderData<SingleAccommodationAvailabilityDetailsWithDeadline>, ProblemDetails>> GetExactAvailability(
            DataProviders dataProvider, long availabilityId, Guid agreementId, string languageCode)
        {
            return await ExecuteRequest()
                .OnSuccess(ApplyMarkup)
                .OnSuccess(SaveToCache)
                .OnSuccess(ReturnResponseWithMarkup)
                .OnSuccess(AddProviderData);


            Task<Result<SingleAccommodationAvailabilityDetailsWithDeadline, ProblemDetails>> ExecuteRequest()
                => _providerRouter.GetExactAvailability(dataProvider, availabilityId, agreementId, languageCode);


            async Task<(SingleAccommodationAvailabilityDetailsWithMarkup, DeadlineDetails)>
                ApplyMarkup(SingleAccommodationAvailabilityDetailsWithDeadline response)
            {
                var customer = await _customerContext.GetCustomer();
                return (await _markupService.Apply(customer,
                        new SingleAccommodationAvailabilityDetails(
                            response.AvailabilityId,
                            response.CheckInDate,
                            response.CheckOutDate,
                            response.NumberOfNights,
                            response.AccommodationDetails,
                            new List<Agreement>
                                {response.Agreement})),
                    response.DeadlineDetails);
            }


            Task SaveToCache((SingleAccommodationAvailabilityDetailsWithMarkup, DeadlineDetails) responseWithDeadline)
            {
                var (availabilityWithMarkup, deadlineDetails) = responseWithDeadline;
                _deadlineDetailsCache.Set(availabilityWithMarkup.ResultResponse.Agreements.Single().Id.ToString(), deadlineDetails);
                return _availabilityResultsCache.Set(dataProvider, availabilityWithMarkup);
            }


            SingleAccommodationAvailabilityDetailsWithDeadline ReturnResponseWithMarkup(
                (SingleAccommodationAvailabilityDetailsWithMarkup, DeadlineDetails) responseWithDeadline)
            {
                var (availabilityWithMarkup, deadlineDetails) = responseWithDeadline;
                var result = availabilityWithMarkup.ResultResponse;
                return new SingleAccommodationAvailabilityDetailsWithDeadline(
                    result.AvailabilityId,
                    result.CheckInDate,
                    result.CheckOutDate,
                    result.NumberOfNights,
                    result.AccommodationDetails,
                    result.Agreements.SingleOrDefault(),
                    deadlineDetails);
            }


            ProviderData<SingleAccommodationAvailabilityDetailsWithDeadline> AddProviderData(SingleAccommodationAvailabilityDetailsWithDeadline availabilityDetails)
                => ProviderData.Create(dataProvider, availabilityDetails);
        }


        private readonly ILocationService _locationService;
        private readonly ICustomerContext _customerContext;
        private readonly IAvailabilityMarkupService _markupService;
        private readonly IAvailabilityResultsCache _availabilityResultsCache;
        private readonly IProviderRouter _providerRouter;
        private readonly IDeadlineDetailsCache _deadlineDetailsCache;
    }
}