using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.AdministratorServices;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Services.Accommodations.Availability;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Mapping;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.BookingEvaluation;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.RoomSelection;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Connectors;
using HappyTravel.Edo.Api.Services.Payments.Accounts;
using HappyTravel.SupplierOptionsProvider;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.DirectApi.Services.Overriden
{
    public class DirectApiBookingEvaluationService : BookingEvaluationService
    {
        public DirectApiBookingEvaluationService(
            ISupplierConnectorManager supplierConnectorManager,
            IBookingEvaluationPriceProcessor priceProcessor,
            IRoomSelectionStorage roomSelectionStorage,
            IAccommodationBookingSettingsService accommodationBookingSettingsService,
            IDateTimeProvider dateTimeProvider,
            IBookingEvaluationStorage bookingEvaluationStorage,
            IAccommodationMapperClient accommodationMapperClient,
            IAdminAgencyManagementService adminAgencyManagementService,
            ILogger<DirectApiBookingEvaluationService> logger,
            IAvailabilityRequestStorage availabilityRequestStorage,
            ISupplierOptionsStorage supplierOptionsStorage,
            IAgentContextService agentContextService,
            IEvaluationTokenStorage tokenStorage,
            IAccountPaymentService accountPaymentService)
            : base(supplierConnectorManager, priceProcessor, roomSelectionStorage, accommodationBookingSettingsService, dateTimeProvider,
                bookingEvaluationStorage,
                accommodationMapperClient, adminAgencyManagementService, logger, availabilityRequestStorage, supplierOptionsStorage, tokenStorage,
                agentContextService, accountPaymentService)
        { }


        protected override Task<Result<Accommodation, ProblemDetails>> GetAccommodation(string htId, string languageCode)
            => Task.FromResult(Result.Success<Accommodation, ProblemDetails>(default));


        protected override SlimAccommodation GetSlimAccommodation(Accommodation accommodation) => default;
    }
}