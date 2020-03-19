using System;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Customers;
using HappyTravel.Edo.Api.Models.Markups;
using HappyTravel.Edo.Api.Services.Connectors;
using HappyTravel.Edo.Api.Services.CurrencyConversion;
using HappyTravel.Edo.Api.Services.Customers;
using HappyTravel.Edo.Api.Services.Locations;
using HappyTravel.Edo.Api.Services.Markups;
using HappyTravel.Edo.Api.Services.PriceProcessing;
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
            ICurrencyConverterService currencyConverterService)
        {
            _locationService = locationService;
            _customerContext = customerContext;
            _markupService = markupService;
            _availabilityResultsCache = availabilityResultsCache;
            _providerRouter = providerRouter;
            _currencyConverterService = currencyConverterService;
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


            Task<Result<CombinedAvailabilityDetails, ProblemDetails>> ExecuteRequest()
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

                return _providerRouter.GetAvailability(location.DataProviders, contract, languageCode)
                    .ToResultWithProblemDetails();
            }


            Task<Result<CombinedAvailabilityDetails, ProblemDetails>> ConvertCurrencies(CombinedAvailabilityDetails availabilityDetails)
                => this.ConvertCurrencies(customer, availabilityDetails, AvailabilityResultsExtensions.ProcessPrices, AvailabilityResultsExtensions.GetCurrency);


            Task<DataWithMarkup<CombinedAvailabilityDetails>> ApplyMarkups(CombinedAvailabilityDetails response) 
                => this.ApplyMarkups(customer, response, AvailabilityResultsExtensions.ProcessPrices);

            
            CombinedAvailabilityDetails ReturnResponseWithMarkup(DataWithMarkup<CombinedAvailabilityDetails> markup) => markup.Data;
        }


        public async Task<Result<ProviderData<SingleAccommodationAvailabilityDetails>, ProblemDetails>> GetAvailable(DataProviders dataProvider,
            string accommodationId, string availabilityId,
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


        public async Task<Result<ProviderData<SingleAccommodationAvailabilityDetailsWithDeadline?>, ProblemDetails>> GetExactAvailability(
            DataProviders dataProvider, string availabilityId, Guid agreementId, string languageCode)
        {
            var customer = await _customerContext.GetCustomer();

            return await ExecuteRequest()
                .OnSuccess(ConvertCurrencies)
                .OnSuccess(ApplyMarkups)
                .OnSuccess(SaveToCache)
                .OnSuccess(AddProviderData);


            Task<Result<SingleAccommodationAvailabilityDetailsWithDeadline?, ProblemDetails>> ExecuteRequest()
                => _providerRouter.GetExactAvailability(dataProvider, availabilityId, agreementId, languageCode);


            Task<Result<SingleAccommodationAvailabilityDetailsWithDeadline?, ProblemDetails>> ConvertCurrencies(SingleAccommodationAvailabilityDetailsWithDeadline? availabilityDetails) => this.ConvertCurrencies(customer,
                availabilityDetails,
                AvailabilityResultsExtensions.ProcessPrices,
                AvailabilityResultsExtensions.GetCurrency);


            Task<DataWithMarkup<SingleAccommodationAvailabilityDetailsWithDeadline?>>
                ApplyMarkups(SingleAccommodationAvailabilityDetailsWithDeadline? response)
                => this.ApplyMarkups(customer, response, AvailabilityResultsExtensions.ProcessPrices);


            Task SaveToCache(DataWithMarkup<SingleAccommodationAvailabilityDetailsWithDeadline?> responseWithDeadline)
            {
                if(!responseWithDeadline.Data.HasValue)
                    return Task.CompletedTask;
                
                return _availabilityResultsCache.Set(dataProvider, DataWithMarkup.Create(responseWithDeadline.Data.Value, 
                    responseWithDeadline.Policies));
            }


            ProviderData<SingleAccommodationAvailabilityDetailsWithDeadline?> AddProviderData(
                DataWithMarkup<SingleAccommodationAvailabilityDetailsWithDeadline?> availabilityDetails)
                => ProviderData.Create(dataProvider, availabilityDetails.Data);
        }


        public Task<Result<ProviderData<DeadlineDetails>, ProblemDetails>> GetDeadlineDetails(
            DataProviders dataProvider, string availabilityId, Guid agreementId, string languageCode)
        {
            return GetDeadline()
                .OnSuccess(AddProviderData);

            Task<Result<DeadlineDetails, ProblemDetails>> GetDeadline() => _providerRouter.GetDeadline(dataProvider,
                availabilityId,
                agreementId, languageCode);

            ProviderData<DeadlineDetails> AddProviderData(DeadlineDetails deadlineDetails)
                => ProviderData.Create(dataProvider, deadlineDetails);
        }
        
        
        private Task<Result<TDetails, ProblemDetails>> ConvertCurrencies<TDetails>(CustomerInfo customer, TDetails details, Func<TDetails, PriceProcessFunction, ValueTask<TDetails>> changePricesFunc, Func<TDetails, Currencies?> getCurrencyFunc)
        {
            return _currencyConverterService
                .ConvertPricesInData(customer, details, changePricesFunc, getCurrencyFunc)
                .ToResultWithProblemDetails();
        }


        private async Task<DataWithMarkup<TDetails>> ApplyMarkups<TDetails>(CustomerInfo customer, TDetails details,
            Func<TDetails, PriceProcessFunction, ValueTask<TDetails>> priceProcessFunc)
        {
            var markup = await _markupService.Get(customer, MarkupPolicyTarget.AccommodationAvailability);
            var responseWithMarkup = await priceProcessFunc(details, markup.Function);
            return DataWithMarkup.Create(responseWithMarkup, markup.Policies);
        }


        private readonly IAvailabilityResultsCache _availabilityResultsCache;
        private readonly ICurrencyConverterService _currencyConverterService;
        private readonly ICustomerContext _customerContext;
        private readonly ILocationService _locationService;
        private readonly IMarkupService _markupService;
        private readonly IProviderRouter _providerRouter;
    }
}