using System;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Customers;
using HappyTravel.Edo.Api.Models.Markups;
using HappyTravel.Edo.Api.Services.Connectors;
using HappyTravel.Edo.Api.Services.CurrencyConversion;
using HappyTravel.Edo.Api.Services.Customers;
using HappyTravel.Edo.Api.Services.Locations;
using HappyTravel.Edo.Api.Services.Markups;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Internals;
using HappyTravel.EdoContracts.General.Enums;
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


            Task<Result<CombinedAvailabilityDetails, ProblemDetails>> ConvertCurrencies(CombinedAvailabilityDetails availabilityDetails)
                => this.ConvertCurrencies(customer, availabilityDetails, AvailabilityResultsExtensions.ProcessPrices, AvailabilityResultsExtensions.GetCurrency);


            Task<DataWithMarkup<CombinedAvailabilityDetails>> ApplyMarkups(CombinedAvailabilityDetails response) 
                => this.ApplyMarkups(customer, response, AvailabilityResultsExtensions.ProcessPrices);

            
            CombinedAvailabilityDetails ReturnResponseWithMarkup(DataWithMarkup<CombinedAvailabilityDetails> markup) => markup.Data;
        }


        public async Task<Result<ProviderData<SingleAccommodationAvailabilityDetails>, ProblemDetails>> GetAvailable(DataProviders dataProvider,
            string accommodationId, long availabilityId,
            string languageCode)
        {
            var customer = await _customerContext.GetCustomer();

            return await ExecuteRequest()
                .OnSuccess(ConvertCurrencies)
                .OnSuccess(ApplyMarkups)
                .OnSuccess(AddProviderData);


            Task<Result<SingleAccommodationAvailabilityDetails, ProblemDetails>> ExecuteRequest()
                => _providerRouter.GetAvailable(dataProvider, accommodationId, availabilityId, languageCode);


            Task<Result<SingleAccommodationAvailabilityDetails, ProblemDetails>> ConvertCurrencies(SingleAccommodationAvailabilityDetails availabilityDetails)
                => this.ConvertCurrencies(customer, availabilityDetails, AvailabilityResultsExtensions.ProcessPrices, AvailabilityResultsExtensions.GetCurrency);


            Task<DataWithMarkup<SingleAccommodationAvailabilityDetails>> ApplyMarkups(SingleAccommodationAvailabilityDetails response) 
                => this.ApplyMarkups(customer, response, AvailabilityResultsExtensions.ProcessPrices);


            ProviderData<SingleAccommodationAvailabilityDetails> AddProviderData(DataWithMarkup<SingleAccommodationAvailabilityDetails> availabilityDetails)
                => ProviderData.Create(dataProvider, availabilityDetails.Data);
        }


        public async Task<Result<ProviderData<SingleAccommodationAvailabilityDetailsWithDeadline>, ProblemDetails>> GetExactAvailability(
            DataProviders dataProvider, long availabilityId, Guid agreementId, string languageCode)
        {
            var customer = await _customerContext.GetCustomer();

            return await ExecuteRequest()
                .OnSuccess(ConvertCurrencies)
                .OnSuccess(ApplyMarkups)
                .OnSuccess(SaveToCache)
                .OnSuccess(AddProviderData);


            Task<Result<SingleAccommodationAvailabilityDetailsWithDeadline, ProblemDetails>> ExecuteRequest()
                => _providerRouter.GetExactAvailability(dataProvider, availabilityId, agreementId, languageCode);


            Task<Result<SingleAccommodationAvailabilityDetailsWithDeadline, ProblemDetails>> ConvertCurrencies(SingleAccommodationAvailabilityDetailsWithDeadline availabilityDetails)
                => this.ConvertCurrencies(customer, availabilityDetails, AvailabilityResultsExtensions.ProcessPrices, AvailabilityResultsExtensions.GetCurrency);

            
            Task<DataWithMarkup<SingleAccommodationAvailabilityDetailsWithDeadline>>
                ApplyMarkups(SingleAccommodationAvailabilityDetailsWithDeadline response)
                => this.ApplyMarkups(customer, response, AvailabilityResultsExtensions.ProcessPrices);


            Task SaveToCache(DataWithMarkup<SingleAccommodationAvailabilityDetailsWithDeadline> responseWithDeadline)
            {
                var deadlineDetails = responseWithDeadline.Data.DeadlineDetails;
                _deadlineDetailsCache.Set(responseWithDeadline.Data.Agreement.Id.ToString(), deadlineDetails);
                return _availabilityResultsCache.Set(dataProvider, responseWithDeadline);
            }


            ProviderData<SingleAccommodationAvailabilityDetailsWithDeadline> AddProviderData(
                DataWithMarkup<SingleAccommodationAvailabilityDetailsWithDeadline> availabilityDetails)
                => ProviderData.Create(dataProvider, availabilityDetails.Data);
        }


        private async Task<Result<TDetails, ProblemDetails>> ConvertCurrencies<TDetails>(CustomerInfo customer, TDetails details,
            Func<TDetails, PriceProcessFunction, ValueTask<TDetails>> processPriceFunc, Func<TDetails, Currencies> getCurrencyFunc)
        {
            var (_, _, settings, _) = await _customerSettingsManager.GetUserSettings(customer);
            var currentCurrency = getCurrencyFunc(details);
            var preferredCurrency = settings.PreferredCurrency;

            if (preferredCurrency == Currencies.NotSpecified || preferredCurrency == currentCurrency)
                return Result.Ok<TDetails, ProblemDetails>(details);
            
            var (_, isFailure, rate, error) = await _currencyRateService.Get(currentCurrency, preferredCurrency);
            if (isFailure)
                return ProblemDetailsBuilder.Fail<TDetails>(error);

            var convertedDetails = await processPriceFunc(details, (price, currency) =>
            {
                var newPrice = price * rate;
                var newCurrency = preferredCurrency;

                return new ValueTask<(decimal, Currencies)>((newPrice, newCurrency));
            });
            
            return Result.Ok<TDetails, ProblemDetails>(convertedDetails);
        }
        
        
        private async Task<DataWithMarkup<TDetails>> ApplyMarkups<TDetails>(CustomerInfo customer, TDetails details,
            Func<TDetails, PriceProcessFunction, ValueTask<TDetails>> func)
        {
            var markup = await _markupService.Get(customer, MarkupPolicyTarget.AccommodationAvailability);
            var responseWithMarkup = await func(details, markup.Function);
            return DataWithMarkup.Create(responseWithMarkup, markup.Policies);
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