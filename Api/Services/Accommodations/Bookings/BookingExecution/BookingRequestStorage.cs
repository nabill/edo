using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Bookings;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.BookingExecution
{
    public class BookingRequestStorage : IBookingRequestStorage
    {
        public BookingRequestStorage(EdoContext edoContext)
        {
            _edoContext = edoContext;
        }
        
        
        public Task Set(string referenceCode, AccommodationBookingRequest request, BookingAvailabilityInfo availabilityInfo)
        {
            var bookingRequest = new BookingRequest
            {
                ReferenceCode = referenceCode,
                RequestData = JsonConvert.SerializeObject(request),
                AvailabilityData = JsonConvert.SerializeObject(availabilityInfo),
            };
            _edoContext.BookingRequests.Add(bookingRequest);
            return _edoContext.SaveChangesAsync();
        }


        public async Task<Result<(AccommodationBookingRequest request, BookingAvailabilityInfo availabilityInfo)>> Get(string referenceCode)
        {
            var request = await _edoContext.BookingRequests
                .SingleOrDefaultAsync(b => b.ReferenceCode == referenceCode);

            if (request is null)
                return Result.Failure<(AccommodationBookingRequest, BookingAvailabilityInfo)>($"Could not get booking request by reference code {referenceCode}");

            var requestData = JsonConvert.DeserializeObject<AccommodationBookingRequest>(request.RequestData);
            var availabilityInfo = JsonConvert.DeserializeObject<BookingAvailabilityInfo>(request.AvailabilityData);
            return (requestData, availabilityInfo);
        }

        
        private readonly EdoContext _edoContext;
    }
}