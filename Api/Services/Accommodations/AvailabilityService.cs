using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Customers;
using HappyTravel.Edo.Api.Models.Markups;
using HappyTravel.Edo.Api.Models.Markups.Availability;
using HappyTravel.Edo.Api.Services.Connectors;
using HappyTravel.Edo.Api.Services.CurrencyConversion;
using HappyTravel.Edo.Api.Services.Customers;
using HappyTravel.Edo.Api.Services.Locations;
using HappyTravel.Edo.Api.Services.Markups;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Internals;
using HappyTravel.EdoContracts.GeoData;
using Microsoft.AspNetCore.Mvc;
using AvailabilityRequest = HappyTravel.Edo.Api.Models.Availabilities.AvailabilityRequest;

namespace HappyTravel.Edo.Api.Services.Accommodations
{
    public class AvailabilityService : IAvailabilityService
    {
        public AvailabilityService(ILocationService locationService,
            ICustomerContext customerContext,
            IMarkupService markupService,
            IAvailabilityResultsCache availabilityResultsCache,
            IProviderRouter providerRouter,
            ICurrencyRateService currencyRateService,
            IDeadlineDetailsCache deadlineDetailsCache,
            ICustomerSettingsManager customerSettingsManager)
        {
            _locationService = locationService;
            _customerContext = customerContext;
            _markupService = markupService;
            _availabilityResultsCache = availabilityResultsCache;
            _providerRouter = providerRouter;
            _currencyRateService = currencyRateService;
            _deadlineDetailsCache = deadlineDetailsCache;
            _customerSettingsManager = customerSettingsManager;
        }


        public async ValueTask<Result<CombinedAvailabilityDetails, ProblemDetails>> GetAvailable(AvailabilityRequest request,
            string languageCode)
        {
            var (_, isFailure, location, error) = await _locationService.Get(request.Location, languageCode);
            if (isFailure)
                return Result.Fail<CombinedAvailabilityDetails, ProblemDetails>(error);

            var customer = await _customerContext.GetCustomer();

            return await ExecuteRequest()
                .OnSuccess(ConvertCurrencies)
                .OnSuccess(ApplyMarkups)
                .OnSuccess(ReturnResponseWithMarkup);


            async Task<Result<CombinedAvailabilityDetails, ProblemDetails>> ExecuteRequest()
            {
                var roomDetails = request.RoomDetails
                    .Select(r => new RoomRequestDetails(r.AdultsNumber, r.ChildrenNumber, r.ChildrenAges, r.Type,
                        r.IsExtraBedNeeded))
                    .ToList();

                var contract = new EdoContracts.Accommodations.AvailabilityRequest(request.Nationality, request.Residency, request.CheckInDate,
                    request.CheckOutDate,
                    request.Filters, roomDetails,
                    new Location(location.Name, location.Locality, location.Country, location.Coordinates, location.Distance, location.Source, location.Type),
                    request.PropertyType, request.Ratings);

                var (isSuccess, _, details, providerError) = await _providerRouter.GetAvailability(location.DataProviders, contract, languageCode);
                return isSuccess
                    ? Result.Ok<CombinedAvailabilityDetails, ProblemDetails>(details)
                    : ProblemDetailsBuilder.Fail<CombinedAvailabilityDetails>(providerError);
            }


            Task<CombinedAvailabilityDetails> ConvertCurrencies(CombinedAvailabilityDetails availabilityDetails)
                => this.ConvertCurrencies(customer, availabilityDetails, AvailabilityResultsExtensions.ProcessPrices);


            async Task<AvailabilityDetailsWithMarkup> ApplyMarkups(CombinedAvailabilityDetails response)
            {
                var markup = await _markupService.Get(customer, MarkupPolicyTarget.AccommodationAvailability);
                var resultResponse = await response.ProcessPrices(markup.Function);
                return new AvailabilityDetailsWithMarkup(markup.Policies, resultResponse);
            }


            CombinedAvailabilityDetails ReturnResponseWithMarkup(AvailabilityDetailsWithMarkup markup) => markup.ResultResponse;
        }


        public async Task<Result<ProviderData<SingleAccommodationAvailabilityDetails>, ProblemDetails>> GetAvailable(DataProviders dataProvider,
            string accommodationId, long availabilityId,
            string languageCode)
        {
            var customer = await _customerContext.GetCustomer();

            return await ExecuteRequest()
                .OnSuccess(ConvertCurrencies)
                .OnSuccess(ApplyMarkup)
                .OnSuccess(ReturnResponseWithMarkup)
                .OnSuccess(AddProviderData);


            Task<Result<SingleAccommodationAvailabilityDetails, ProblemDetails>> ExecuteRequest()
                => _providerRouter.GetAvailable(dataProvider, accommodationId, availabilityId, languageCode);


            Task<SingleAccommodationAvailabilityDetails> ConvertCurrencies(SingleAccommodationAvailabilityDetails availabilityDetails)
                => this.ConvertCurrencies(customer, availabilityDetails, AvailabilityResultsExtensions.ProcessPrices);


            async Task<SingleAccommodationAvailabilityDetailsWithMarkup> ApplyMarkup(SingleAccommodationAvailabilityDetails response)
            {
                var markup = await _markupService.Get(customer, MarkupPolicyTarget.AccommodationAvailability);
                var responseWithMarkup = await response.ProcessPrices(markup.Function);
                return new SingleAccommodationAvailabilityDetailsWithMarkup(markup.Policies, responseWithMarkup);
            }


            SingleAccommodationAvailabilityDetails ReturnResponseWithMarkup(SingleAccommodationAvailabilityDetailsWithMarkup markup) => markup.ResultResponse;


            ProviderData<SingleAccommodationAvailabilityDetails> AddProviderData(SingleAccommodationAvailabilityDetails availabilityDetails)
                => ProviderData.Create(dataProvider, availabilityDetails);
        }


        public async Task<Result<ProviderData<SingleAccommodationAvailabilityDetailsWithDeadline>, ProblemDetails>> GetExactAvailability(
            DataProviders dataProvider, long availabilityId, Guid agreementId, string languageCode)
        {
            var customer = await _customerContext.GetCustomer();

            return await ExecuteRequest()
                .OnSuccess(ConvertCurrencies)
                .OnSuccess(ApplyMarkup)
                .OnSuccess(SaveToCache)
                .OnSuccess(ReturnResponseWithMarkup)
                .OnSuccess(AddProviderData);


            Task<Result<SingleAccommodationAvailabilityDetailsWithDeadline, ProblemDetails>> ExecuteRequest()
                => _providerRouter.GetExactAvailability(dataProvider, availabilityId, agreementId, languageCode);


            Task<SingleAccommodationAvailabilityDetailsWithDeadline> ConvertCurrencies(SingleAccommodationAvailabilityDetailsWithDeadline availabilityDetails)
                => this.ConvertCurrencies(customer, availabilityDetails, AvailabilityResultsExtensions.ProcessPrices);


            async Task<(SingleAccommodationAvailabilityDetailsWithMarkup, DeadlineDetails)>
                ApplyMarkup(SingleAccommodationAvailabilityDetailsWithDeadline response)
            {
                var markup = await _markupService.Get(customer, MarkupPolicyTarget.AccommodationAvailability);
                var responseWithMarkup = await response.ProcessPrices(markup.Function);
                var resultResponse = new SingleAccommodationAvailabilityDetails(
                    response.AvailabilityId,
                    response.CheckInDate,
                    response.CheckOutDate,
                    response.NumberOfNights,
                    response.AccommodationDetails,
                    new List<Agreement> {responseWithMarkup.Agreement});

                return (new SingleAccommodationAvailabilityDetailsWithMarkup(markup.Policies, resultResponse), response.DeadlineDetails);
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


            ProviderData<SingleAccommodationAvailabilityDetailsWithDeadline> AddProviderData(
                SingleAccommodationAvailabilityDetailsWithDeadline availabilityDetails)
                => ProviderData.Create(dataProvider, availabilityDetails);
        }


        private async Task<TDetails> ConvertCurrencies<TDetails>(CustomerInfo customer, TDetails tDetails,
            Func<TDetails, PriceProcessFunction, ValueTask<TDetails>> func)
        {
            var (_, _, settings, _) = await _customerSettingsManager.GetUserSettings(customer);
            var preferredCurrency = settings.PreferredCurrency;

            if (preferredCurrency == default)
                return tDetails;

            return await func(tDetails, async (price, currency) =>
            {
                var newCurrency = preferredCurrency;
                var newPrice = price * await _currencyRateService.Get(currency, preferredCurrency);
                return (newPrice, newCurrency);
            });
        }


        private readonly IAvailabilityResultsCache _availabilityResultsCache;
        private readonly ICurrencyRateService _currencyRateService;
        private readonly ICustomerContext _customerContext;
        private readonly ICustomerSettingsManager _customerSettingsManager;
        private readonly IDeadlineDetailsCache _deadlineDetailsCache;
        private readonly ILocationService _locationService;
        private readonly IMarkupService _markupService;
        private readonly IProviderRouter _providerRouter;
    }
}