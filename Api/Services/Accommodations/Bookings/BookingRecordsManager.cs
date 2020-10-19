using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Services.Accommodations.Availability;
using HappyTravel.Edo.Api.Services.CodeProcessors;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Booking;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Internals;
using HappyTravel.EdoContracts.General.Enums;
using HappyTravel.Money.Models;
using Microsoft.EntityFrameworkCore;
using Booking = HappyTravel.Edo.Data.Booking.Booking;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings
{
    internal class BookingRecordsManager : IBookingRecordsManager
    {
        public BookingRecordsManager(EdoContext context,
            IDateTimeProvider dateTimeProvider,
            ITagProcessor tagProcessor,
            IAccommodationService accommodationService,
            IAccommodationBookingSettingsService accommodationBookingSettingsService)
        {
            _context = context;
            _dateTimeProvider = dateTimeProvider;
            _tagProcessor = tagProcessor;
            _accommodationService = accommodationService;
            _accommodationBookingSettingsService = accommodationBookingSettingsService;
        }


        public async Task<string> Register(AccommodationBookingRequest bookingRequest,
            BookingAvailabilityInfo availabilityInfo, AgentContext agentContext, string languageCode)
        {
            var (_, _, booking, _) = await Result.Success()
                .Map(GetTags)
                .Map(Create);

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


            async Task<Data.Booking.Booking> Create((string itn, string referenceCode) tags)
            {
                var createdBooking = BookingFactory.Create(
                    _dateTimeProvider.UtcNow(),
                    agentContext,
                    tags.itn,
                    tags.referenceCode,
                    availabilityInfo,
                    bookingRequest.PaymentMethod,
                    bookingRequest,
                    languageCode,
                    availabilityInfo.DataProvider,
                    availabilityInfo.RoomContractSet.Deadline.Date,
                    availabilityInfo.CheckInDate,
                    availabilityInfo.CheckOutDate);

                _context.Bookings.Add(createdBooking);
                await _context.SaveChangesAsync();
                _context.Entry(createdBooking).State = EntityState.Detached;

                return createdBooking;
            }
        }


        public async Task UpdateBookingDetails(EdoContracts.Accommodations.Booking bookingDetails, Data.Booking.Booking booking)
        {
            booking.SupplierReferenceCode = bookingDetails.AgentReference;
            booking.Status = bookingDetails.Status.ToInternalStatus();
            booking.UpdateMode = bookingDetails.BookingUpdateMode;
            booking.Rooms = MergeRemarks(booking.Rooms, bookingDetails.RoomContractSet.RoomContracts);
            
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();
            _context.Entry(booking).State = EntityState.Detached;


            static List<BookedRoom> MergeRemarks(List<BookedRoom> bookedRooms, List<RoomContract> roomContracts)
            {
                // TODO: NIJO-928 Find corresponding room in more solid way
                // We cannot find corresponding room if room count differs
                if (roomContracts == null || bookedRooms.Count != roomContracts.Count)
                    return bookedRooms;
                
                var changedBookedRooms = new List<BookedRoom>(bookedRooms.Count);
                for (var i = 0; i < roomContracts.Count; i++)
                {
                    var correspondingRoom = bookedRooms[i];
                    var remarksToChange = correspondingRoom.Remarks;

                    foreach (var newRemark in roomContracts[i].Remarks)
                    {
                        if (!remarksToChange.Contains(newRemark))
                            remarksToChange.Add(newRemark);
                    }
                        
                    var changedBookedRoom = new BookedRoom(correspondingRoom, remarksToChange.ToList());
                    changedBookedRooms.Add(changedBookedRoom);
                }

                return changedBookedRooms;
            }
        }


        public Task<List<AgentBoundedData<SlimAccommodationBookingInfo>>> GetAgencyBookingsInfo(AgentContext agentContext)
        {
            return (from booking in _context.Bookings
                join agent in _context.Agents on booking.AgentId equals agent.Id
                where booking.AgencyId == agentContext.AgencyId
                let bookingInfo = new SlimAccommodationBookingInfo(booking)
                let agentInfo = new SlimAgentDescription(agent.Id, agent.FirstName, agent.LastName, agent.Position)
                select new AgentBoundedData<SlimAccommodationBookingInfo>(agentInfo, bookingInfo))
                .ToListAsync();
        }


        public Task Confirm(EdoContracts.Accommodations.Booking bookingDetails, Data.Booking.Booking booking)
        {
            booking.ConfirmationDate = _dateTimeProvider.UtcNow();
            return UpdateBookingDetails(bookingDetails, booking);
        }


        public async Task ConfirmBookingCancellation(Data.Booking.Booking booking)
        {
            if (booking.PaymentStatus == BookingPaymentStatuses.Authorized)
                booking.PaymentStatus = BookingPaymentStatuses.Voided;
            if (booking.PaymentStatus == BookingPaymentStatuses.Captured)
                booking.PaymentStatus = BookingPaymentStatuses.Refunded;

            booking.Status = BookingStatuses.Cancelled;
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
                ? Result.Failure<Data.Booking.Booking>("Could not get booking data")
                : Result.Success(booking);
        }


        public async Task<Result<Data.Booking.Booking>> GetAgentsBooking(string referenceCode, AgentContext agentContext)
        {
            return await Get(booking => agentContext.AgentId == booking.AgentId && booking.ReferenceCode == referenceCode);
        }


        public async Task<Result<AccommodationBookingInfo>> GetAgentAccommodationBookingInfo(int bookingId, AgentContext agentContext, string languageCode)
        {
            var bookingDataResult = await Get(booking => agentContext.AgentId == booking.AgentId && booking.Id == bookingId);
            if (bookingDataResult.IsFailure)
                return Result.Failure<AccommodationBookingInfo>(bookingDataResult.Error);

            var (_, isFailure, bookingInfo, error) = await ConvertToBookingInfo(bookingDataResult.Value, languageCode, agentContext);
            if(isFailure)
                return Result.Failure<AccommodationBookingInfo>(error);

            return Result.Success(bookingInfo);
        }


        public async Task<Result<AccommodationBookingInfo>> GetAgentAccommodationBookingInfo(string referenceCode, AgentContext agentContext, string languageCode)
        {
            var bookingDataResult = await Get(booking => agentContext.AgentId == booking.AgentId && booking.ReferenceCode == referenceCode);
            if (bookingDataResult.IsFailure)
                return Result.Failure<AccommodationBookingInfo>(bookingDataResult.Error);

            var (_, isFailure, bookingInfo, error) = await ConvertToBookingInfo(bookingDataResult.Value, languageCode, agentContext);
            if(isFailure)
                return Result.Failure<AccommodationBookingInfo>(error);

            return bookingInfo;
        }

        
        public async Task<Result<AccommodationBookingInfo>> GetAccommodationBookingInfo(string referenceCode, string languageCode)
        {
            var bookingDataResult = await Get(booking => booking.ReferenceCode == referenceCode);
            if (bookingDataResult.IsFailure)
                return Result.Failure<AccommodationBookingInfo>(bookingDataResult.Error);
            
            var (_, isFailure, bookingInfo, error) = await ConvertToBookingInfo(bookingDataResult.Value, languageCode);
            if(isFailure)
                return Result.Failure<AccommodationBookingInfo>(error);

            return bookingInfo;
        }

        /// <summary>
        /// Gets all booking info of the current agent
        /// </summary>
        /// <returns>List of the slim booking models </returns>
        public async Task<Result<List<SlimAccommodationBookingInfo>>> GetAgentBookingsInfo(AgentContext agentContext)
        {
            var bookingData = await _context.Bookings
                .Where(b => b.AgentId == agentContext.AgentId)
                .Where(b => 
                    (b.PaymentMethod == PaymentMethods.BankTransfer)
                    || (b.PaymentMethod != PaymentMethods.BankTransfer && b.PaymentStatus != BookingPaymentStatuses.NotPaid))
                .Select(b =>
                    new SlimAccommodationBookingInfo(b)
                ).ToListAsync();

            return Result.Success(bookingData);
        }


        public async Task<Result> SetPaymentMethod(string referenceCode, PaymentMethods paymentMethod)
        {
            return await Get(referenceCode)
                .Tap(SetPaymentMethod);


            async Task SetPaymentMethod(Booking booking)
            {
                if (booking.PaymentMethod == paymentMethod)
                    return;

                booking.PaymentMethod = paymentMethod;

                _context.Update(booking);
                await _context.SaveChangesAsync();

                _context.Entry(booking).State = EntityState.Detached;
            }
        }


        public async Task SetNeedsManualCorrectionStatus(Data.Booking.Booking booking)
        {
            booking.Status = BookingStatuses.ManualCorrectionNeeded;
            _context.Update(booking);
            await _context.SaveChangesAsync();
            _context.Entry(booking).State = EntityState.Detached;
        }

        
        private async Task<Result<AccommodationBookingInfo>> ConvertToBookingInfo(Booking booking, string languageCode, AgentContext? agentContext = null)
        {
            var (_, isFailure, accommodation, error) = await _accommodationService.Get(booking.DataProvider, booking.AccommodationId, languageCode);
            if(isFailure)
                return Result.Failure<AccommodationBookingInfo>(error.Detail);
            
            var bookingDetails = GetDetails(booking, accommodation);
            var provider = await GetDataProvider(booking, agentContext);
            var agentInformation = await GetAgentInformation(booking.AgentId, booking.AgencyId);

            return new AccommodationBookingInfo(booking.Id,
                bookingDetails,
                booking.CounterpartyId,
                booking.PaymentStatus,
                new MoneyAmount(booking.TotalPrice, booking.Currency),
                provider,
                agentInformation);


            static AccommodationBookingDetails GetDetails(Data.Booking.Booking booking, Accommodation accommodationDetails)
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
            
            
            async Task<DataProviders?> GetDataProvider(Booking booking, AgentContext? agent)
            {
                if (agent == null)
                    return booking.DataProvider;
            
                var settings = await _accommodationBookingSettingsService.Get(agent.Value);
                var provider = settings.IsDataProviderVisible
                    ? booking.DataProvider
                    : (DataProviders?) null;
                return provider;
            }
            
            
            Task<AccommodationBookingInfo.BookingAgentInformation> GetAgentInformation(int agentId, int agencyId)
            {
                var agencyInfoQuery = from agent in _context.Agents
                    join relation in _context.AgentAgencyRelations on agent.Id equals relation.AgentId
                    join agency in _context.Agencies on relation.AgencyId equals agency.Id
                    join counterparty in _context.Counterparties on agency.CounterpartyId equals counterparty.Id
                    where agent.Id == booking.AgentId && agency.Id == booking.AgencyId
                    let agentName = $"{agent.FirstName} {agent.LastName}"
                    select new AccommodationBookingInfo.BookingAgentInformation(agentName,
                        agency.Name, counterparty.Name, agent.Email);

                return agencyInfoQuery.SingleOrDefaultAsync();
            }
        }


        // TODO: Replace method when will be added other services 
        private Task<bool> AreExistBookingsForItn(string itn, int agentId)
            => _context.Bookings.Where(b => b.AgentId == agentId && b.ItineraryNumber == itn).AnyAsync();


        private readonly EdoContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ITagProcessor _tagProcessor;
        private readonly IAccommodationService _accommodationService;
        private readonly IAccommodationBookingSettingsService _accommodationBookingSettingsService;
    }
}