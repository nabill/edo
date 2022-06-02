using System;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Mapping;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.BookingExecution;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Payments;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.CodeProcessors;
using HappyTravel.Edo.Api.Services.SupplierOrders;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Bookings;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.DirectApi.Services.Overriden
{
    public class DirectApiBookingRegistrationService : BookingRegistrationService
    {
        public DirectApiBookingRegistrationService(EdoContext context, 
            ITagProcessor tagProcessor, 
            IDateTimeProvider dateTimeProvider, 
            IAppliedBookingMarkupRecordsManager appliedBookingMarkupRecordsManager, 
            IBookingChangeLogService changeLogService, 
            ISupplierOrderService supplierOrderService, 
            IBookingRequestStorage requestStorage, 
            IAccommodationMapperClient accommodationMapperClient,
            ILogger<DirectApiBookingRegistrationService> logger,
            IAgentContextService agentContextService) 
            : base(context, tagProcessor, dateTimeProvider, appliedBookingMarkupRecordsManager, changeLogService, supplierOrderService, requestStorage, logger, agentContextService)
        {
            _accommodationMapperClient = accommodationMapperClient;
        }


        protected override async Task<Booking> AddStaticData(Booking booking, BookingAvailabilityInfo availabilityInfo)
        {
            var (_, isFailure, accommodation, error) = await _accommodationMapperClient.GetAccommodation(availabilityInfo.HtId, booking.LanguageCode ?? "en");
            if (isFailure)
            {
                throw new Exception($"Cannot get accommodation for '{availabilityInfo.HtId}' with error `{error.Detail}`");
            }

            var edoAccommodation = accommodation.ToEdoContract();
            var location = edoAccommodation.Location;

            booking.Location = new AccommodationLocation(location.Country,
                location.Locality,
                location.LocalityZone,
                location.Address,
                location.Coordinates);

            booking.AccommodationId = edoAccommodation.Id;
            booking.AccommodationName = edoAccommodation.Name;

            if (accommodation.Photos.Any())
            {
                booking.AccommodationInfo = new Data.Bookings.AccommodationInfo(
                    new ImageInfo(edoAccommodation.Photos[0].Caption, edoAccommodation.Photos[0].SourceUrl));
            }

            return booking;
        }


        private readonly IAccommodationMapperClient _accommodationMapperClient;
    }
}