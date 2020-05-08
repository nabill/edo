using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Services.CodeProcessors;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Enums;
using HappyTravel.Money.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings
{
    internal class BookingRecordsManager : IBookingRecordsManager
    {
        public BookingRecordsManager(EdoContext context,
            IDateTimeProvider dateTimeProvider,
            IAgentContext agentContext,
            ITagProcessor tagProcessor,
            ILogger<BookingRecordsManager> logger)
        {
            _context = context;
            _dateTimeProvider = dateTimeProvider;
            _agentContext = agentContext;
            _tagProcessor = tagProcessor;
            _logger = logger;
        }


        public async Task<Result<string>> Register(AccommodationBookingRequest bookingRequest, BookingAvailabilityInfo availabilityInfo, string languageCode)
        {
            var (_, isAgentFailure, agentInfo, agentError) = await _agentContext.GetAgentInfo();

            return isAgentFailure
                ? ProblemDetailsBuilder.Fail<string>(agentError)
                : Result.Ok(await CreateBooking());


            async Task<string> CreateBooking()
            {
                var tags = await GetTags();
                var initialBooking = new BookingBuilder()
                    .AddCreationDate(_dateTimeProvider.UtcNow())
                    .AddAgentInfo(agentInfo)
                    .AddTags(tags.itn, tags.referenceCode)
                    .AddStatus(BookingStatusCodes.InternalProcessing)
                    .AddServiceDetails(availabilityInfo)
                    .AddPaymentMethod(bookingRequest.PaymentMethod)
                    .AddRequestInfo(bookingRequest)
                    .AddLanguageCode(languageCode)
                    .AddProviderInfo(bookingRequest.DataProvider)
                    .AddPaymentStatus(BookingPaymentStatuses.NotPaid)
                    .Build();

                _context.Bookings.Add(initialBooking);

                await _context.SaveChangesAsync();

                return tags.referenceCode;
            }


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

                    if (!await AreExistBookingsForItn(itn, agentInfo.CounterpartyId))
                        itn = await _tagProcessor.GenerateItn();
                }

                var referenceCode = await _tagProcessor.GenerateReferenceCode(
                    ServiceTypes.HTL,
                    availabilityInfo.CountryCode,
                    itn);

                return (itn, referenceCode);
            }
        }


        public async Task<BookingDetails> Finalize(
            Data.Booking.Booking booking,
            BookingDetails bookingDetails)
        {
            var bookingEntity = new BookingBuilder(booking)
                .AddBookingDetails(bookingDetails)
                .AddStatus(bookingDetails.Status)
                .AddBookingDate(_dateTimeProvider.UtcNow())
                .AddUpdateMode(bookingDetails.BookingUpdateMode)
                .Build();
            
            _context.Bookings.Update(bookingEntity);
            await _context.SaveChangesAsync();
            _context.Entry(bookingEntity).State = EntityState.Detached;
            return bookingDetails;
        }


        public async Task UpdateBookingDetails(BookingDetails bookingDetails, Data.Booking.Booking booking)
        {
            booking.Status = bookingDetails.Status;
            booking.SupplierReferenceCode = bookingDetails.BookingCode;
            booking.DeadlineDate = bookingDetails.Deadline;
            booking.CheckInDate = bookingDetails.CheckInDate;
            booking.CheckOutDate = bookingDetails.CheckOutDate;
            booking.SupplierReferenceCode = bookingDetails.AgentReference;

            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();
            _context.Entry(booking).State = EntityState.Detached;
        }


        public Task Confirm(BookingDetails bookingDetails, Data.Booking.Booking booking)
        {
            booking.BookingDate = _dateTimeProvider.UtcNow();
            return UpdateBookingDetails(bookingDetails, booking);
        }


        public async Task ConfirmBookingCancellation(Data.Booking.Booking booking)
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


        public Task<Result<Data.Booking.Booking>> Get(string referenceCode)
        {
            return Get(booking => booking.ReferenceCode == referenceCode);
        }


        public Task<Result<Data.Booking.Booking>> Get(int bookingId)
        {
            return Get(booking => booking.Id == bookingId);
        }

        
        public Task<Result<Data.Booking.Booking>> Get(int bookingId, int agentId)
        {
            return Get(booking => booking.Id == bookingId && booking.AgentId == agentId);
        }
        

        private async Task<Result<Data.Booking.Booking>> Get(Expression<Func<Data.Booking.Booking, bool>> filterExpression)
        {
            var booking = await _context.Bookings
                .Where(filterExpression)
                .SingleOrDefaultAsync();

            return booking == default
                ? Result.Fail<Data.Booking.Booking>("Could not get booking data")
                : Result.Ok(booking);
        }


        public async Task<Result<Data.Booking.Booking>> GetAgentsBooking(string referenceCode)
        {
            var (_, isAgentFailure, agentData, agentError) = await _agentContext.GetAgentInfo();
            if (isAgentFailure)
                return Result.Fail<Data.Booking.Booking>(agentError);

            return await Get(booking => agentData.AgentId == booking.AgentId && booking.ReferenceCode == referenceCode);
        }


        public async Task<Result<AccommodationBookingInfo>> GetAgentBookingInfo(int bookingId)
        {
            var agentData = await _agentContext.GetAgent();

            var bookingDataResult = await Get(booking => agentData.AgentId == booking.AgentId && booking.Id == bookingId);
            if (bookingDataResult.IsFailure)
                return Result.Fail<AccommodationBookingInfo>(bookingDataResult.Error);

            return Result.Ok(ConvertToBookingInfo(bookingDataResult.Value));
        }


        public async Task<Result<AccommodationBookingInfo>> GetAgentBookingInfo(string referenceCode)
        {
            var agentData = await _agentContext.GetAgent();

            var bookingDataResult = await Get(booking => agentData.AgentId == booking.AgentId && booking.ReferenceCode == referenceCode);
            if (bookingDataResult.IsFailure)
                return Result.Fail<AccommodationBookingInfo>(bookingDataResult.Error);

            return Result.Ok(ConvertToBookingInfo(bookingDataResult.Value));
        }


        /// <summary>
        /// Gets all booking info of the current agent
        /// </summary>
        /// <returns>List of the slim booking models </returns>
        public async Task<Result<List<SlimAccommodationBookingInfo>>> GetAgentBookingsInfo()
        {
            var agentData = await _agentContext.GetAgent();

            var bookingData = await _context.Bookings
                .Where(b => b.AgentId == agentData.AgentId)
                .Select(b =>
                    new SlimAccommodationBookingInfo(b)
                ).ToListAsync();

            return Result.Ok(bookingData);
        }


        private AccommodationBookingInfo ConvertToBookingInfo(Data.Booking.Booking booking)
        {
            var bookingDetails = GetDetails();

            return new AccommodationBookingInfo(booking.Id,
                bookingDetails,
                booking.CounterpartyId,
                booking.PaymentStatus,
                new MoneyAmount(booking.TotalPrice, booking.Currency),
                booking.Rooms);


            AccommodationBookingDetails GetDetails()
            {
                return new AccommodationBookingDetails(booking.ReferenceCode,
                    booking.Status,
                    booking.CheckInDate,
                    booking.CheckOutDate,
                    booking.Location,
                    booking.AccommodationId,
                    booking.AccommodationName,
                    booking.DeadlineDate,
                    booking.Rooms);
            }
        }
        
        
        // TODO: Replace method when will be added other services 
        private Task<bool> AreExistBookingsForItn(string itn, int agentId)
            => _context.Bookings.Where(b => b.AgentId == agentId && b.ItineraryNumber == itn).AnyAsync();


        private readonly EdoContext _context;
        private readonly IAgentContext _agentContext;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ITagProcessor _tagProcessor;
        private readonly ILogger<BookingRecordsManager> _logger;
    }
}