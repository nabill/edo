using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.AdministratorServices;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Services.Accommodations;
using HappyTravel.Edo.Api.Services.Accommodations.Availability;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.BookingEvaluation;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.RoomSelection;
using HappyTravel.Edo.Api.Services.Connectors;
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
            IAccommodationService accommodationService,
            IAdminAgencyManagementService adminAgencyManagementService, 
            ILogger<DirectApiBookingEvaluationService> logger) 
            : base(supplierConnectorManager, priceProcessor, roomSelectionStorage, accommodationBookingSettingsService, dateTimeProvider, bookingEvaluationStorage,
                accommodationService, adminAgencyManagementService, logger)
        {
            
        }


        protected override Task<Result<Accommodation, ProblemDetails>> GetAccommodation(string htId, string languageCode) 
            => Task.FromResult(Result.Success<Accommodation, ProblemDetails>(default));


        protected override SlimAccommodation GetSlimAccommodation(Accommodation accommodation) 
            => default;
    }
}