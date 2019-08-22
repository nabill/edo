using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Services.Customers;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Booking;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Accommodations
{
    public class AccommodationBookingManager : IAccommodationBookingManager
    {
        public AccommodationBookingManager(IOptions<DataProviderOptions> options,
            IDataProviderClient dataProviderClient, 
            EdoContext context,
            IAvailabilityResultsCache availabilityResultsCache,
            IDateTimeProvider dateTimeProvider,
            ICustomerContext customerContext)
        {
            _dataProviderClient = dataProviderClient;
            _options = options.Value;
            _context = context;
            _availabilityResultsCache = availabilityResultsCache;
            _dateTimeProvider = dateTimeProvider;
            _customerContext = customerContext;
        }

        public async Task<Result<AccommodationBookingDetails, ProblemDetails>> Book(AccommodationBookingRequest bookingRequest,
            string languageCode)
        {
            var (_, isFailure, customer, error) = await  _customerContext.GetCurrent();
            if (isFailure)
                return ProblemDetailsBuilder.BuildFailResult<AccommodationBookingDetails>(error);
            
            var availability = await GetSelectedAvailabilityInfo(bookingRequest.AvailabilityId, bookingRequest.AgreementId);
            if(availability.Equals(default))
                return ProblemDetailsBuilder.BuildFailResult<AccommodationBookingDetails>("Could not find availability by given id");

            var itn = bookingRequest.ItineraryNumber ?? await _context.GetNextItineraryNumber();
            var referenceCode = ReferenceCodeGenerator.Generate(ServiceTypes.HTL,
                availability.SelectedResult.AccommodationDetails.Location.CountryCode,
                itn);
            
            var inner = new InnerAccommodationBookingRequest(bookingRequest, 
                availability, referenceCode);

            return await ExecuteBookingRequest(inner)
                .OnSuccess(confirmedBooking => SaveBookingResults(bookingRequest,
                    confirmedBooking,
                    availability,
                    itn,
                    customer.Id));
            
            async ValueTask<BookingAvailabilityInfo> GetSelectedAvailabilityInfo(int availabilityId, Guid agreementId)
            {
                var availabilityResponse = await _availabilityResultsCache.Get(availabilityId);
                if (availabilityResponse.Equals(default))
                    return default;
                    
                return (from availabilityResult in availabilityResponse.Results
                        from agreement in availabilityResult.Agreements
                        where agreement.Id == agreementId
                        select new BookingAvailabilityInfo(availabilityResponse, availabilityResult, agreement))
                    .SingleOrDefault();
            }

            Task<Result<AccommodationBookingDetails, ProblemDetails>> ExecuteBookingRequest(in InnerAccommodationBookingRequest innerRequest)
            {
                return _dataProviderClient.Post<InnerAccommodationBookingRequest, AccommodationBookingDetails>(
                    new Uri(_options.Netstorming + "hotels/booking", UriKind.Absolute),
                    innerRequest, languageCode);
            }
        }

        private async Task SaveBookingResults(AccommodationBookingRequest bookingRequest, AccommodationBookingDetails confirmedBooking,
            BookingAvailabilityInfo selectedAvailabilityInfo, long itineraryNumber, int customerId)
        {
            var booking = CreateBooking();
            _context.AccommodationBookings.Add(booking);

            await _context.SaveChangesAsync();

            AccommodationBooking CreateBooking()
            {
                return new AccommodationBooking
                {
                    BookingDate = _dateTimeProvider.UtcNow(),
                    Deadline = confirmedBooking.Deadline,
                    Status = confirmedBooking.Status,
                    AccommodationId = selectedAvailabilityInfo.SelectedResult.AccommodationDetails.Id,
                    ReferenceCode = confirmedBooking.ReferenceCode,
                
                    Service = selectedAvailabilityInfo.SelectedResult.AccommodationDetails.Name,
                    TariffCode = selectedAvailabilityInfo.SelectedAgreement.TariffCode,
                    ContractTypeId = confirmedBooking.ContractTypeId,
                
                    AgentReference = bookingRequest.AgentReference,
                    Nationality = bookingRequest.Nationality,
                    Residency = bookingRequest.Residency,

                    CheckInDate = confirmedBooking.CheckInDate,
                    CheckOutDate = confirmedBooking.CheckOutDate,
                    RateBasis = selectedAvailabilityInfo.SelectedAgreement.BoardBasis,
                
                    PriceCurrency = Enum.Parse<Currencies>(selectedAvailabilityInfo.SelectedAgreement.CurrencyCode), 
                    CountryCode = selectedAvailabilityInfo.SelectedResult.AccommodationDetails.Location.CountryCode,
                    CityCode = selectedAvailabilityInfo.SelectedResult.AccommodationDetails.Location.CityCode,
                    Features = selectedAvailabilityInfo.SelectedAgreement.Remarks,
                
                    CustomerId = customerId,
                    RoomDetails = CreateRoomDetails(confirmedBooking.RoomDetails),
                    
                    ItineraryNumber = itineraryNumber
                };
            }

            List<AccomodationBookingRoomDetails> CreateRoomDetails(List<BookingRoomDetailsWithPrice> roomDetails)
            {
                return roomDetails.Select(r => new AccomodationBookingRoomDetails()
                    {
                        Price = r.Price.Price,
                        CotPrice = r.Price.CotPrice,
                        ExtraBedPrice = r.Price.ExtraBedPrice,
                        Type = r.RoomDetails.Type,
                        IsCotNeededNeeded = r.RoomDetails.IsCotNeededNeeded,
                        IsExtraBedNeeded = r.RoomDetails.IsExtraBedNeeded,
                        Passengers = r.RoomDetails.Passengers.Select(p => new AccomodationBookingPassenger()
                        {
                            FirstName = p.FirstName,
                            LastName = p.LastName,
                            Title = p.Title,
                            Initials = p.Initials,
                            Age = p.Age,
                            IsLeader = p.IsLeader
                        }).ToList()
                    })
                    .ToList();
            }
        }

        private readonly EdoContext _context;
        private readonly IAvailabilityResultsCache _availabilityResultsCache;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ICustomerContext _customerContext;
        private readonly IDataProviderClient _dataProviderClient;
        private readonly DataProviderOptions _options;
    }
}