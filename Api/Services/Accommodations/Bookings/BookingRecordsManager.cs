using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Services.CodeProcessors;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Booking;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Enums;
using HappyTravel.Money.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings
{
    internal class BookingRecordsManager : IBookingRecordsManager
    {
        public BookingRecordsManager(EdoContext context,
            IDateTimeProvider dateTimeProvider,
            ITagProcessor tagProcessor,
            IAccommodationService accommodationService,
            ILogger<BookingRecordsManager> logger)
        {
            _context = context;
            _dateTimeProvider = dateTimeProvider;
            _tagProcessor = tagProcessor;
            _accommodationService = accommodationService;
            _logger = logger;
        }


        public async Task<string> Register(AccommodationBookingRequest bookingRequest,
            BookingAvailabilityInfo availabilityInfo, AgentContext agentContext, string languageCode)
        {
            var (_, _, booking, _) = await Result.Success()
                .Map(GetTags)
                .Map(Book);

            return booking.ReferenceCode;


            async Task<(string itn, string referenceCode)> GetTags()
            {
                string itn;
                if (string.IsNullOrWhiteSpace(bookingRequest.ItineraryNumber))
                {
                    itn = await _tagProcessor.GenerateItn();
                }
                else
                {
                    // User can send reference code instead of itn
                    if (!_tagProcessor.TryGetItnFromReferenceCode(bookingRequest.ItineraryNumber, out itn))
                        itn = bookingRequest.ItineraryNumber;

                    if (!await AreExistBookingsForItn(itn, agentContext.AgentId))
                        itn = await _tagProcessor.GenerateItn();
                }

                var referenceCode = await _tagProcessor.GenerateReferenceCode(
                    ServiceTypes.HTL,
                    availabilityInfo.CountryCode,
                    itn);

                return (itn, referenceCode);
            }


            async Task<Booking> Book((string itn, string referenceCode) tags)
            {
                var createdBooking = BookingFactory.Build(
                    _dateTimeProvider.UtcNow(),
                    agentContext,
                    tags.itn,
                    tags.referenceCode,
                    BookingStatusCodes.InternalProcessing,
                    availabilityInfo,
                    bookingRequest.PaymentMethod,
                    bookingRequest,
                    languageCode,
                    availabilityInfo.DataProvider,
                    BookingPaymentStatuses.NotPaid);

                _context.Bookings.Add(createdBooking);
                await _context.SaveChangesAsync();

                return createdBooking;
            }
        }


        public async Task UpdateBookingDetails(BookingDetails bookingDetails, Booking booking)
        {
            booking.AddBookingDetails(bookingDetails);
            
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();
            _context.Entry(booking).State = EntityState.Detached;
        }


        public Task Confirm(BookingDetails bookingDetails, Booking booking)
        {
            booking.BookingDate = _dateTimeProvider.UtcNow();
            return UpdateBookingDetails(bookingDetails, booking);
        }


        public async Task ConfirmBookingCancellation(Booking booking)
        {
            if (booking.PaymentStatus == BookingPaymentStatuses.Authorized)
                booking.PaymentStatus = BookingPaymentStatuses.Voided;
            if (booking.PaymentStatus == BookingPaymentStatuses.Captured)
                booking.PaymentStatus = BookingPaymentStatuses.Refunded;

            booking.Status = BookingStatusCodes.Cancelled;
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();
            _context.Entry(booking).State = EntityState.Detached;
        }


        public Task<Result<Booking>> Get(string referenceCode)
        {
            return Get(booking => booking.ReferenceCode == referenceCode);
        }


        public Task<Result<Booking>> Get(int bookingId)
        {
            return Get(booking => booking.Id == bookingId);
        }

        
        public Task<Result<Booking>> Get(int bookingId, int agentId)
        {
            return Get(booking => booking.Id == bookingId && booking.AgentId == agentId);
        }
        

        private async Task<Result<Booking>> Get(Expression<Func<Booking, bool>> filterExpression)
        {
            var booking = await _context.Bookings
                .Where(filterExpression)
                .SingleOrDefaultAsync();

            return booking == default
                ? Result.Failure<Booking>("Could not get booking data")
                : Result.Ok(booking);
        }


        public async Task<Result<Booking>> GetAgentsBooking(string referenceCode, AgentContext agentContext)
        {
            return await Get(booking => agentContext.AgentId == booking.AgentId && booking.ReferenceCode == referenceCode);
        }


        public async Task<Result<AccommodationBookingInfo>> GetAgentAccommodationBookingInfo(int bookingId, AgentContext agentContext, string languageCode)
        {
            var bookingDataResult = await Get(booking => agentContext.AgentId == booking.AgentId && booking.Id == bookingId);
            if (bookingDataResult.IsFailure)
                return Result.Failure<AccommodationBookingInfo>(bookingDataResult.Error);

            var (_, isFailure, bookingInfo, error) = await ConvertToBookingInfo(bookingDataResult.Value, languageCode);
            if(isFailure)
                return Result.Failure<AccommodationBookingInfo>(error);

            return Result.Ok(bookingInfo);
        }


        public async Task<Result<AccommodationBookingInfo>> GetAgentAccommodationBookingInfo(string referenceCode, AgentContext agentContext, string languageCode)
        {
            var bookingDataResult = await Get(booking => agentContext.AgentId == booking.AgentId && booking.ReferenceCode == referenceCode);
            if (bookingDataResult.IsFailure)
                return Result.Failure<AccommodationBookingInfo>(bookingDataResult.Error);

            var (_, isFailure, bookingInfo, error) = await ConvertToBookingInfo(bookingDataResult.Value, languageCode);
            if(isFailure)
                return Result.Failure<AccommodationBookingInfo>(error);

            return Result.Ok(bookingInfo);
        }


        /// <summary>
        /// Gets all booking info of the current agent
        /// </summary>
        /// <returns>List of the slim booking models </returns>
        public async Task<Result<List<SlimAccommodationBookingInfo>>> GetAgentBookingsInfo(AgentContext agentContext)
        {
            var bookingData = await _context.Bookings
                .Where(b => b.AgentId == agentContext.AgentId)
                .Where(b => b.PaymentStatus != BookingPaymentStatuses.NotPaid)
                .Select(b =>
                    new SlimAccommodationBookingInfo(b)
                ).ToListAsync();

            return Result.Ok(bookingData);
        }


        private async Task<Result<AccommodationBookingInfo>> ConvertToBookingInfo(Booking booking, string languageCode)
        {
            var (_, isFailure, accommodation, error) = await _accommodationService.Get(booking.DataProvider, booking.AccommodationId, languageCode);
            if(isFailure)
                return Result.Failure<AccommodationBookingInfo>(error.Detail);
            
            var bookingDetails = GetDetails(accommodation);

            return Result.Ok(new AccommodationBookingInfo(booking.Id,
                bookingDetails,
                booking.CounterpartyId,
                booking.PaymentStatus,
                new MoneyAmount(booking.TotalPrice, booking.Currency)));


            AccommodationBookingDetails GetDetails(AccommodationDetails accommodationDetails)
            {
                var passengerNumber = booking.Rooms.Sum(r => r.Passengers.Count);
                var numberOfNights = (booking.CheckOutDate - booking.CheckInDate).Days;
                return new AccommodationBookingDetails(booking.ReferenceCode,
                    booking.SupplierReferenceCode,
                    booking.Status,
                    numberOfNights,
                    booking.CheckInDate,
                    booking.CheckOutDate,
                    booking.Location,
                    accommodationDetails.Contacts,
                    booking.AccommodationId,
                    booking.AccommodationName,
                    booking.DeadlineDate,
                    booking.Rooms,
                    passengerNumber);
            }
        }
        
        
        // TODO: Replace method when will be added other services 
        private Task<bool> AreExistBookingsForItn(string itn, int agentId)
            => _context.Bookings.Where(b => b.AgentId == agentId && b.ItineraryNumber == itn).AnyAsync();


        private readonly EdoContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ITagProcessor _tagProcessor;
        private readonly IAccommodationService _accommodationService;
        private readonly ILogger<BookingRecordsManager> _logger;
    }
}