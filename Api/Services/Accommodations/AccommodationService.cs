using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Availabilities;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Services.Customers;
using HappyTravel.Edo.Api.Services.Deadline;
using HappyTravel.Edo.Api.Services.Locations;
using HappyTravel.Edo.Api.Services.Markups;
using HappyTravel.Edo.Api.Services.Markups.Availability;
using HappyTravel.Edo.Common.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Accommodations
{
    public class AccommodationService : IAccommodationService
    {
        public AccommodationService(IMemoryFlow flow,
            IOptions<DataProviderOptions> options,
            IDataProviderClient dataProviderClient,
            ILocationService locationService,
            IAccommodationBookingManager accommodationBookingManager,
            IAvailabilityResultsCache availabilityResultsCache,
            ICustomerContext customerContext,
            IAvailabilityMarkupService markupService,
            ICancelationPoliciesService cancelationPoliciesService)
        {
            _flow = flow;
            _dataProviderClient = dataProviderClient;
            _locationService = locationService;
            _accommodationBookingManager = accommodationBookingManager;
            _availabilityResultsCache = availabilityResultsCache;
            _customerContext = customerContext;
            _markupService = markupService;
            _options = options.Value;
            _cancelationPoliciesService = cancelationPoliciesService;
        }


        public ValueTask<Result<RichAccommodationDetails, ProblemDetails>> Get(string accommodationId, string languageCode)
            => _flow.GetOrSetAsync(_flow.BuildKey(nameof(AccommodationService), "Accommodations", languageCode, accommodationId),
                async () => await _dataProviderClient.Get<RichAccommodationDetails>(new Uri($"{_options.Netstorming}hotels/{accommodationId}", UriKind.Absolute),
                    languageCode),
                TimeSpan.FromDays(1));


        public async ValueTask<Result<AvailabilityResponse, ProblemDetails>> GetAvailable(AvailabilityRequest request, string languageCode)
        {
            var (_, isFailure, location, error) = await _locationService.Get(request.Location, languageCode);
            if (isFailure)
                return Result.Fail<AvailabilityResponse, ProblemDetails>(error);

            var (_, isCustomerFailure, customerInfo, customerError) = await _customerContext.GetCustomerInfo();
            if (isCustomerFailure)
                return ProblemDetailsBuilder.Fail<AvailabilityResponse>(customerError);

            return await ExecuteRequest()
                .OnSuccess(ApplyMarkup)
                .OnSuccess(SaveToCache)
                .OnSuccess(ReturnResponseWithMarkup);

            Task<Result<AvailabilityResponse, ProblemDetails>> ExecuteRequest() => _dataProviderClient.Post<InnerAvailabilityRequest, AvailabilityResponse>(
                new Uri(_options.Netstorming + "hotels/availability", UriKind.Absolute),
                new InnerAvailabilityRequest(request, location), languageCode);

            Task<AvailabilityResponseWithMarkup> ApplyMarkup(AvailabilityResponse response)
            {
                return _markupService.Apply(customerInfo, response);
            }

            Task SaveToCache(AvailabilityResponseWithMarkup response) => _availabilityResultsCache.Set(response);

            AvailabilityResponse ReturnResponseWithMarkup(AvailabilityResponseWithMarkup markup) => markup.ResultResponse;
        }


        public async Task<Result<AccommodationBookingDetails, ProblemDetails>> Book(AccommodationBookingRequest request, string languageCode)
        {
            var bookingAvailability = await GetBookingAvailability(request.AvailabilityId, request.AgreementId, languageCode);
            if (bookingAvailability.IsFailure)
                return ProblemDetailsBuilder.Fail<AccommodationBookingDetails>(bookingAvailability.Error.Detail);

            // TODO: add storing markup and supplier price
            return await _accommodationBookingManager.Book(
                request,
                bookingAvailability.Value,
                languageCode);
        }


        public async Task<Result<BookingAvailabilityInfo, ProblemDetails>> GetBookingAvailability(int availabilityId, Guid agreementId, string languageCode)
        {
            var availabilityResponse = await _availabilityResultsCache.Get(availabilityId);
            var availability = ExtractBookingAvailabilityInfo(availabilityResponse.ResultResponse, agreementId);
            if (availability.Equals(default))
                return ProblemDetailsBuilder.Fail<BookingAvailabilityInfo>("Could not find availability by given id");

            var deadlineDetailsResponse = await _cancelationPoliciesService.GetDeadlineDetails(
                availabilityId.ToString(),
                availability.AccommodationId,
                availability.Agreement.TariffCode,
                DataProvidersContractTypes.Netstorming,
                languageCode);

            if (deadlineDetailsResponse.IsFailure)
                return ProblemDetailsBuilder.Fail<BookingAvailabilityInfo>($"Could not get deadline policies: {deadlineDetailsResponse.Error.Detail}");
            return Result.Ok<BookingAvailabilityInfo, ProblemDetails>(availability.AddDeadlineDetails(deadlineDetailsResponse.Value));
        }

        private BookingAvailabilityInfo ExtractBookingAvailabilityInfo(AvailabilityResponse response, Guid agreementId)
        {
            if (response.Equals(default))
                return default;

            return (from availabilityResult in response.Results
                    from agreement in availabilityResult.Agreements
                    where agreement.Id == agreementId
                    select new BookingAvailabilityInfo(
                        availabilityResult.AccommodationDetails.Id,
                        availabilityResult.AccommodationDetails.Name,
                        agreement,
                        availabilityResult.AccommodationDetails.Location.CityCode,
                        availabilityResult.AccommodationDetails.Location.City,
                        availabilityResult.AccommodationDetails.Location.CountryCode,
                        availabilityResult.AccommodationDetails.Location.Country,
                        response.CheckInDate,
                        response.CheckOutDate))
                .SingleOrDefault();
        }

        public Task<List<AccommodationBookingInfo>> GetBookings()
        {
            return _accommodationBookingManager.Get();
        }

        public Task<Result<VoidObject, ProblemDetails>> CancelBooking(int bookingId)
        {
            return _accommodationBookingManager.Cancel(bookingId);
        }

        private readonly IDataProviderClient _dataProviderClient;
        private readonly IMemoryFlow _flow;
        private readonly ILocationService _locationService;
        private readonly IAccommodationBookingManager _accommodationBookingManager;
        private readonly IAvailabilityResultsCache _availabilityResultsCache;
        private readonly ICustomerContext _customerContext;
        private readonly IAvailabilityMarkupService _markupService;
        private readonly DataProviderOptions _options;
        private readonly ICancelationPoliciesService _cancelationPoliciesService;
    }
}