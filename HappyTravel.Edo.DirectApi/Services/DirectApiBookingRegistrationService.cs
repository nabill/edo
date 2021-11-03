using System;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Services.Accommodations;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.BookingExecution;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Payments;
using HappyTravel.Edo.Api.Services.CodeProcessors;
using HappyTravel.Edo.Api.Services.SupplierOrders;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Bookings;

namespace HappyTravel.Edo.DirectApi.Services
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
            IAccommodationService accommodationService) 
            : base(context, tagProcessor, dateTimeProvider, appliedBookingMarkupRecordsManager, changeLogService, supplierOrderService, requestStorage)
        {
            _accommodationService = accommodationService;
        }


        protected override async Task<Booking> AddStaticData(Booking booking, BookingAvailabilityInfo availabilityInfo)
        {
            var (_, isFailure, accommodation, error) = await _accommodationService.Get(availabilityInfo.HtId, booking.LanguageCode ?? "en");
            if (isFailure)
            {
                throw new Exception($"Cannot get accommodation for '{availabilityInfo.HtId}' with error `{error.Detail}`");
            }

            var location = accommodation.Location;

            booking.Location = new AccommodationLocation(location.Country,
                location.Locality,
                location.LocalityZone,
                location.Address,
                location.Coordinates);

            booking.AccommodationId = accommodation.Id;
            booking.AccommodationName = accommodation.Name;

            if (accommodation.Photos.Any())
            {
                booking.AccommodationInfo = new Data.Bookings.AccommodationInfo(
                    new ImageInfo(accommodation.Photos[0].Caption, accommodation.Photos[0].SourceUrl));
            }

            return booking;
        }


        private readonly IAccommodationService _accommodationService;
    }
}