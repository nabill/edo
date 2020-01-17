using System;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Booking;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Internals;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Accommodations
{
    public class BookingRequestDataLogService: IBookingRequestDataLogService
    {
        public BookingRequestDataLogService(EdoContext edoContext)
        {
            _edoContext = edoContext;
        }
        
          
        public async Task<Result<BookingRequestDataEntry>> Get(string referenceCode)
        {
            var requestLogs = await _edoContext.BookingRequestLogs
                .Where(le => _edoContext.Bookings
                    .Where(b => b.ReferenceCode.Equals(referenceCode))
                    .Select(i => i.Id)
                    .Contains(le.BookingId)).ToListAsync();
           
           return !requestLogs.Any() 
               ? Result.Fail<BookingRequestDataEntry>("Failed to get a booking request by the reference code") 
               : Result.Ok(requestLogs.First());
        }


        public async Task<Result<BookingRequestDataEntry>> Get(int bookingId)
        {
            var requestLogs = await _edoContext.BookingRequestLogs
                .Where(le => le.BookingId.Equals(bookingId))
                .ToListAsync();
            
            return !requestLogs.Any() 
                ? Result.Fail<BookingRequestDataEntry>("Failed to get a booking request by the reference code") 
                : Result.Ok(requestLogs.First());
        }
        

        public async Task<Result> Add(string bookingReferenceCode, AccommodationBookingRequest bookingRequest, string languageCode, DataProviders dataProvider)
        {
            var booking = await _edoContext.Bookings.SingleOrDefaultAsync(i => i.ReferenceCode.Equals(bookingReferenceCode));
            
            if (booking == null || booking.Id.Equals(default))
                return Result.Fail<BookingRequestDataEntry>("Failed to add booking request data");

            var roomDetails= bookingRequest.RoomDetails.Select(rd
                => new SlimRoomDetails(rd.Type, rd.Passengers)).ToList();

            var features = bookingRequest.Features.Select(ft 
                => new Feature(Enum.GetName(typeof(AccommodationFeatureTypes), ft.Type),
                    ft.Value)).ToList(); 
            
            await _edoContext.BookingRequestLogs.AddAsync(new BookingRequestDataEntry
            {
                BookingId = booking.Id,
                CustomerId = booking.CustomerId,
                BookingRequest = new BookingRequest(
                    bookingRequest.AvailabilityId,
                    bookingRequest.AgreementId,
                    bookingRequest.Nationality,
                    bookingRequest.PaymentMethod,
                    bookingReferenceCode,
                    bookingRequest.Residency,
                    roomDetails,
                    features,
                    bookingRequest.RejectIfUnavailable
                    ),
                LanguageCode = languageCode
            });
            await _edoContext.SaveChangesAsync();
            return Result.Ok();
        }


        private EdoContext _edoContext ;
    }
}