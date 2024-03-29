using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.AdministratorServices;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Markups;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Mapping;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.RoomSelection;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Connectors;
using HappyTravel.Edo.Api.Services.Payments.Accounts;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.SupplierOptionsProvider;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RoomContractSetAvailability = HappyTravel.Edo.Api.Models.Accommodations.RoomContractSetAvailability;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.BookingEvaluation
{
    public class BookingEvaluationService : IBookingEvaluationService
    {
        public BookingEvaluationService(ISupplierConnectorManager supplierConnectorManager,
            IBookingEvaluationPriceProcessor priceProcessor,
            IRoomSelectionStorage roomSelectionStorage,
            IAccommodationBookingSettingsService accommodationBookingSettingsService,
            IDateTimeProvider dateTimeProvider,
            IBookingEvaluationStorage bookingEvaluationStorage,
            IAccommodationMapperClient accommodationMapperClient,
            IAdminAgencyManagementService adminAgencyManagementService,
            ILogger<BookingEvaluationService> logger,
            IAvailabilityRequestStorage availabilityRequestStorage,
            ISupplierOptionsStorage supplierOptionsStorage,
            IEvaluationTokenStorage tokenStorage,
            IAgentContextService agentContext, 
            IAccountPaymentService accountPaymentService)
        {
            _supplierConnectorManager = supplierConnectorManager;
            _priceProcessor = priceProcessor;
            _roomSelectionStorage = roomSelectionStorage;
            _accommodationBookingSettingsService = accommodationBookingSettingsService;
            _dateTimeProvider = dateTimeProvider;
            _bookingEvaluationStorage = bookingEvaluationStorage;
            _accommodationMapperClient = accommodationMapperClient;
            _adminAgencyManagementService = adminAgencyManagementService;
            _logger = logger;
            _availabilityRequestStorage = availabilityRequestStorage;
            _supplierOptionsStorage = supplierOptionsStorage;
            _agentContext = agentContext;
            _tokenStorage = tokenStorage;
            _accountPaymentService = accountPaymentService;
        }


        public async Task<Result<RoomContractSetAvailability?, ProblemDetails>> GetExactAvailability(
            Guid searchId, string htId, Guid roomContractSetId, string languageCode)
        {
            Baggages.AddSearchId(searchId);
            var agent = await _agentContext.GetAgent();
            var settings = await _accommodationBookingSettingsService.Get();
            var (_, isFailure, selectedRoomSet, error) = await GetSelectedRoomSet(searchId, htId, roomContractSetId);
            if (isFailure)
                return ProblemDetailsBuilder.Fail<RoomContractSetAvailability?>(error);

            var availabilityRequest = await _availabilityRequestStorage.Get(searchId);
            if (availabilityRequest.IsFailure)
                return ProblemDetailsBuilder.Fail<RoomContractSetAvailability?>(availabilityRequest.Error);

            var connectorEvaluationResult = await EvaluateOnConnector(selectedRoomSet);
            if (connectorEvaluationResult.IsFailure)
            {
                _logger.LogBookingEvaluationFailure(connectorEvaluationResult.Error.Status, connectorEvaluationResult.Error.Detail);
                return (RoomContractSetAvailability?)null;
            }

            if (connectorEvaluationResult.Value is null)
                return (RoomContractSetAvailability?)null;

            var originalSupplierPrice = connectorEvaluationResult.Value.Value.RoomContractSet.Rate.FinalPrice;

            var (_, isContractFailure, contractKind, contractError) = await _adminAgencyManagementService.GetContractKind(agent.AgencyId);
            if (isContractFailure)
                return ProblemDetailsBuilder.Fail<RoomContractSetAvailability?>(contractError);

            var accommodationResult = await GetAccommodation(selectedRoomSet.htId, languageCode);
            if (accommodationResult.IsFailure)
            {
                _logger.LogGetAccommodationByHtIdFailed(selectedRoomSet.htId, accommodationResult.Error.Detail);
                return (RoomContractSetAvailability?)null;
            }

            var slimAccommodation = GetSlimAccommodation(accommodationResult.Value);

            return await Convert(connectorEvaluationResult.Value.Value)
                .Bind(ConvertCurrencies)
                .Map(ProcessPolicies)
                .Map(ApplyMarkups)
                .Map(AlignPrices)
                .Tap(SaveToCache)
                .Map(e => e.Data)
                .Check(CheckAgainstSettings)
                .Check(CheckCancellationPolicies)
                .Map(ApplySearchSettings);


            async Task<Result<(string SupplierCode, RoomContractSet RoomContractSet, string AvailabilityId, string htId, 
                    string CountryHtId, string LocalityHtId, int MarketId, string CountryCode)>>
                GetSelectedRoomSet(Guid searchId, string htId, Guid roomContractSetId)
            {
                var result = (await _roomSelectionStorage.GetResult(searchId, htId, settings.EnabledConnectors))
                    .SelectMany(r =>
                    {
                        return r.Result.RoomContractSets
                            .Select(rs => (Source: r.SupplierCode, RoomContractSet: rs, r.Result.AvailabilityId, r.Result.HtId,
                                CountryHtId: r.Result.CountryHtId, LocalityHtId: r.Result.LocalityHtId, r.Result.MarketId, r.Result.CountryCode));
                    })
                    .SingleOrDefault(r => r.RoomContractSet.Id == roomContractSetId);

                if (result.Equals(default))
                    return Result.Failure<(string, RoomContractSet, string, string, string, string, int, string)>("Could not find selected room contract set");

                return result;
            }


            Task<Result<EdoContracts.Accommodations.RoomContractSetAvailability?, ProblemDetails>>
                EvaluateOnConnector((string, RoomContractSet, string, string, string, string, int, string) selectedSet)
            {
                var (supplier, roomContractSet, availabilityId, _, _, _, _, _) = selectedSet;
                return _supplierConnectorManager
                    .Get(supplier)
                    .GetExactAvailability(availabilityId, roomContractSet.Id, languageCode);
            }


            async Task<Result<RoomContractSetAvailability, ProblemDetails>> Convert(EdoContracts.Accommodations.RoomContractSetAvailability availabilityData)
            {
                var (_, supplierFailure, supplier, supplierError) = _supplierOptionsStorage.Get(selectedRoomSet.SupplierCode);
                if (supplierFailure)
                    return ProblemDetailsBuilder.Fail<RoomContractSetAvailability>(supplierError);

                var (_, balanceFailure, balanceResult, balanceError) =
                    await _accountPaymentService.GetAccountBalance(selectedRoomSet.RoomContractSet.Rate.FinalPrice.Currency, agent.AgencyId);
                if (balanceFailure)
                    return ProblemDetailsBuilder.Fail<RoomContractSetAvailability>(balanceError);

                var isBalanceEnough = balanceResult.Balance >= selectedRoomSet.RoomContractSet.Rate.FinalPrice.Amount;

                var paymentMethods = GetAvailablePaymentTypes(availabilityData, contractKind, isBalanceEnough);

                var evaluationToken = await _tokenStorage.GetAndSet(availabilityData.RoomContractSet.Id);
                return availabilityData.ToRoomContractSetAvailability(supplier.Name,
                    supplierCode: supplier.Code,
                    paymentMethods: paymentMethods,
                    accommodation: slimAccommodation,
                    countryHtId: selectedRoomSet.CountryHtId,
                    localityHtId: selectedRoomSet.LocalityHtId,
                    evaluationToken: evaluationToken,
                    marketId: selectedRoomSet.MarketId,
                    countryCode: selectedRoomSet.CountryCode);
            }


            Task<Result<RoomContractSetAvailability, ProblemDetails>> ConvertCurrencies(RoomContractSetAvailability availabilityDetails)
                => _priceProcessor.ConvertCurrencies(availabilityDetails);


            RoomContractSetAvailability ProcessPolicies(RoomContractSetAvailability availabilityDetails)
                => BookingEvaluationPolicyProcessor.Process(availabilityDetails, settings.CancellationPolicyProcessSettings);


            async Task<DataWithMarkup<RoomContractSetAvailability>> ApplyMarkups(RoomContractSetAvailability response)
            {
                var appliedMarkups = new List<AppliedMarkup>();
                var convertedSupplierPrice = response.RoomContractSet.Rate.FinalPrice;
                // Saving all the changes in price that was done by markups
                Action<MarkupApplicationResult<RoomContractSetAvailability>> logAction = appliedMarkup =>
                {
                    var markupAmount = appliedMarkup.After.RoomContractSet.Rate.FinalPrice - appliedMarkup.Before.RoomContractSet.Rate.FinalPrice;
                    var policy = appliedMarkup.Policy;
                    int? agentId = null, agencyId = null;
                    switch (appliedMarkup.Policy.SubjectScopeType)
                    {
                        case SubjectMarkupScopeTypes.Agent:
                            var agentInAgencyId = AgentInAgencyId.Create(policy.SubjectScopeId);
                            agentId = agentInAgencyId.AgentId;
                            agencyId = agentInAgencyId.AgencyId;
                            break;
                        case SubjectMarkupScopeTypes.Agency:
                            agencyId = int.Parse(policy.SubjectScopeId);
                            break;
                    }

                    appliedMarkups.Add(new AppliedMarkup(
                        scope: new MarkupPolicyScope(policy.SubjectScopeType, agencyId, agentId),
                        policyId: policy.Id,
                        amountChange: markupAmount
                    ));
                };

                var responseWithMarkups = await _priceProcessor.ApplyMarkups(response, agent, logAction);
                return DataWithMarkup.Create(responseWithMarkups, appliedMarkups, convertedSupplierPrice, originalSupplierPrice);
            }


            async Task<DataWithMarkup<RoomContractSetAvailability>> AlignPrices(DataWithMarkup<RoomContractSetAvailability> availabilityWithMarkup)
            {
                var processedData = await _priceProcessor.AlignPrices(availabilityWithMarkup.Data, agent);
                return new DataWithMarkup<RoomContractSetAvailability>(processedData,
                    availabilityWithMarkup.AppliedMarkups,
                    availabilityWithMarkup.ConvertedSupplierPrice,
                    originalSupplierPrice);
            }


            Task SaveToCache(DataWithMarkup<RoomContractSetAvailability> responseWithDeadline)
            {
                // TODO: Check that this id will not change on all connectors NIJO-823
                var finalRoomContractSet = responseWithDeadline.Data.RoomContractSet;

                var dataWithMarkup = DataWithMarkup.Create(responseWithDeadline.Data,
                    responseWithDeadline.AppliedMarkups, responseWithDeadline.ConvertedSupplierPrice, responseWithDeadline.OriginalSupplierPrice);

                var agentDeadline = DeadlineMerger.CalculateMergedDeadline(finalRoomContractSet.Rooms);
                var supplierDeadline = DeadlineMerger.CalculateMergedDeadline(connectorEvaluationResult.Value.Value.RoomContractSet.RoomContracts);
                var creditCardRequirement = connectorEvaluationResult.Value.Value.CreditCardRequirement;

                return _bookingEvaluationStorage.Set(searchId: searchId,
                    roomContractSetId: finalRoomContractSet.Id,
                    htId: htId,
                    availability: dataWithMarkup,
                    agentDeadline: agentDeadline,
                    supplierDeadline: supplierDeadline,
                    cardRequirement: creditCardRequirement.HasValue
                        ? new CreditCardRequirement(creditCardRequirement.Value.ActivationDate.DateTime, creditCardRequirement.Value.DueDate.DateTime)
                        : null,
                    supplierAccommodationCode: connectorEvaluationResult.Value.Value.AccommodationId,
                    availabilityRequest: availabilityRequest.Value);
            }


            Result<Unit, ProblemDetails> CheckAgainstSettings(RoomContractSetAvailability availabilityValue)
            {
                return RoomContractSetSettingsChecker.IsEvaluationAllowed(availabilityValue.RoomContractSet, availabilityValue.CheckInDate, settings,
                    _dateTimeProvider)
                    ? Unit.Instance
                    : ProblemDetailsBuilder.Fail<Unit>("You can't book the contract within deadline without explicit approval from a Happytravel.com officer.");
            }


            Result<Unit, ProblemDetails> CheckCancellationPolicies(RoomContractSetAvailability availabilityValue)
            {
                // We need to perform such a check because there were cases, when cancellation policies with 0% penalty came from connectors, which is incorrect
                var deadline = availabilityValue.RoomContractSet.Deadline;

                var isInvalid = deadline is null || deadline.Policies.Any(p => p.Percentage == 0d);

                if (isInvalid)
                {
                    _logger.LogBookingEvaluationCancellationPoliciesFailure();
                    return ProblemDetailsBuilder.Fail<Unit>("Error in cancellation policies data");
                }

                return Unit.Instance;
            }


            RoomContractSetAvailability? ApplySearchSettings(RoomContractSetAvailability availability)
            {
                var roomContractSet = availability.RoomContractSet.ApplySearchSettings(settings.IsSupplierVisible, settings.IsDirectContractFlagVisible);
                return new RoomContractSetAvailability(availabilityId: availability.AvailabilityId,
                    checkInDate: availability.CheckInDate,
                    checkOutDate: availability.CheckOutDate,
                    numberOfNights: availability.NumberOfNights,
                    accommodation: availability.Accommodation,
                    roomContractSet: roomContractSet,
                    availablePaymentMethods: availability.AvailablePaymentMethods,
                    countryHtId: availability.CountryHtId,
                    localityHtId: availability.LocalityHtId,
                    evaluationToken: availability.EvaluationToken,
                    marketId: availability.MarketId,
                    countryCode: availability.CountryCode,
                    supplierCode: availability.SupplierCode);
            }


            List<PaymentTypes> GetAvailablePaymentTypes(in EdoContracts.Accommodations.RoomContractSetAvailability availability,
                in ContractKind contractKind, bool isBalanceEnough)
                => BookingPaymentTypesHelper.GetAvailablePaymentTypes(availability, settings, contractKind, _dateTimeProvider.UtcNow(), isBalanceEnough);
        }


        protected virtual async Task<Result<Accommodation, ProblemDetails>> GetAccommodation(string htId, string languageCode)
        {
            var (_, isFailure, accommodation, problemDetails) = await _accommodationMapperClient.GetAccommodation(htId, languageCode);

            return isFailure
                ? problemDetails
                : accommodation.ToEdoContract();
        }


        protected virtual SlimAccommodation GetSlimAccommodation(Accommodation accommodation)
        {
            var location = accommodation.Location;
            return new SlimAccommodation(location: new SlimLocationInfo(address: location.Address,
                    country: location.Country,
                    countryCode: location.CountryCode,
                    locality: location.Locality,
                    localityZone: location.LocalityZone,
                    coordinates: location.Coordinates),
                name: accommodation.Name,
                photo: accommodation.Photos.FirstOrDefault(),
                rating: accommodation.Rating,
                propertyType: accommodation.Type,
                htId: accommodation.HtId,
                hotelChain: accommodation.HotelChain);
        }


        private readonly ISupplierConnectorManager _supplierConnectorManager;
        private readonly IBookingEvaluationPriceProcessor _priceProcessor;
        private readonly IRoomSelectionStorage _roomSelectionStorage;
        private readonly IAccommodationBookingSettingsService _accommodationBookingSettingsService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IBookingEvaluationStorage _bookingEvaluationStorage;
        private readonly IAccommodationMapperClient _accommodationMapperClient;
        private readonly IAdminAgencyManagementService _adminAgencyManagementService;
        private readonly ILogger<BookingEvaluationService> _logger;
        private readonly IAvailabilityRequestStorage _availabilityRequestStorage;
        private readonly ISupplierOptionsStorage _supplierOptionsStorage;
        private readonly IAgentContextService _agentContext;
        private readonly IEvaluationTokenStorage _tokenStorage;
        private readonly IAccountPaymentService _accountPaymentService;
    }
}