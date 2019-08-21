using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Accommodations;
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
        public AccommodationBookingManager(IOptions<DataProviderOptions> options, IDataProviderClient dataProviderClient, 
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

        public async Task<Result<AccommodationBookingDetails, ProblemDetails>> Book(AccommodationBookingRequest request,
            string languageCode)
        {
            var (_, isFailure, customer, error) = await  _customerContext.GetCurrent();
            if (isFailure)
                return ProblemDetailsBuilder.BuildFailResult<AccommodationBookingDetails>(error);
            
            var itn = await _context.GetNextItineraryNumber();
            var referenceCode = ReferenceCodeGenerator.Generate(ServiceTypes.HTL, request.Residency, itn);

            var inner = new InnerAccommodationBookingRequest(request, referenceCode);

            return await ExecuteBookingRequest(inner)
                .OnSuccess(booking => SaveBookingResults(booking, request, customer.Id));

            Task<Result<AccommodationBookingDetails, ProblemDetails>> ExecuteBookingRequest(in InnerAccommodationBookingRequest innerRequest)
            {
                return _dataProviderClient.Post<InnerAccommodationBookingRequest, AccommodationBookingDetails>(
                    new Uri(_options.Netstorming + "hotels/booking", UriKind.Absolute),
                    innerRequest, languageCode);
            }
        }

        private async Task SaveBookingResults(AccommodationBookingDetails bookedDetails,
            AccommodationBookingRequest request, int customerId)
        {
            var availabilityResponse = await _availabilityResultsCache.Get(request.AvailabilityId);
            var (chosenResult, chosenAgreement) = (from availabilityResult in availabilityResponse.Results
                    from agreement in availabilityResult.Agreements
                    where agreement.Id == request.AgreementId
                    select (availabilityResult, agreement))
                .Single();
                
            var booking = CreateBooking(bookedDetails, chosenResult.AccommodationDetails);
            _context.AccommodationBookings.Add(booking);

            await _context.SaveChangesAsync();

            AccommodationBooking CreateBooking(AccommodationBookingDetails details, SlimAccommodationDetails accommodationDetails)
            {
                return new AccommodationBooking
                {
                    BookingDate = _dateTimeProvider.UtcNow(),
                    Deadline = details.Deadline,
                    Status = details.Status,
                    AccommodationId = details.AccommodationId,
                    ReferenceCode = details.ReferenceCode,
                
                    Service = accommodationDetails.Name,
                    TariffCode = details.TariffCode,
                    ContractTypeId = details.ContractTypeId,
                
                    // Location
                    AgentReference = request.AgentReference,
                    Nationality = request.Nationality,
                    Residency = request.Residency,

                    CheckInDate = details.CheckInDate,
                    CheckOutDate = details.CheckOutDate,
                    RateBasis = chosenAgreement.BoardBasis,
                
                    PriceCurrency = Enum.Parse<Currencies>(chosenAgreement.CurrencyCode), 
                    CountryCode = accommodationDetails.Location.CountryCode,
                    CityCode = accommodationDetails.Location.CityCode,
                    Features = chosenAgreement.Remarks,
                
                    CustomerId = customerId,
                    RoomDetails = CreateRoomDetails(details.RoomDetails)
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