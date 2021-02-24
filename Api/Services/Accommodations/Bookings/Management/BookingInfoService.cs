using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Services.Accommodations.Availability;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.General.Enums;
using HappyTravel.Money.Models;
using Microsoft.EntityFrameworkCore;
using Booking = HappyTravel.Edo.Data.Bookings.Booking;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management
{
    public class BookingInfoService : IBookingInfoService
    {
        public BookingInfoService(EdoContext context, 
            IBookingRecordManager bookingRecordManager,
            IAccommodationService accommodationService,
            IAccommodationBookingSettingsService accommodationBookingSettingsService)
        {
            _context = context;
            _bookingRecordManager = bookingRecordManager;
            _accommodationService = accommodationService;
            _accommodationBookingSettingsService = accommodationBookingSettingsService;
        }
        
        
        public IQueryable<AgentBoundedData<SlimAccommodationBookingInfo>> GetAgencyBookingsInfo(AgentContext agentContext)
        {
            return from booking in _context.Bookings
                join agent in _context.Agents on booking.AgentId equals agent.Id
                where booking.AgencyId == agentContext.AgencyId
                select new AgentBoundedData<SlimAccommodationBookingInfo>
                {
                    Agent = new SlimAgentDescription
                    {
                        Id = agent.Id,
                        FirstName = agent.FirstName,
                        LastName = agent.LastName,
                        Position = agent.Position
                    },
                    Data = new SlimAccommodationBookingInfo
                    {
                        Id = booking.Id,
                        ReferenceCode = booking.ReferenceCode,
                        AccommodationName = booking.AccommodationName,
                        CountryName = booking.Location.Country,
                        LocalityName = booking.Location.Locality,
                        Deadline = booking.DeadlineDate,
                        Price = new MoneyAmount(booking.TotalPrice, booking.Currency),
                        CheckInDate = booking.CheckInDate,
                        CheckOutDate = booking.CheckOutDate,
                        Status = booking.Status,
                        PaymentStatus = booking.PaymentStatus,
                        Rooms = booking.Rooms,
                        Supplier = booking.Supplier
                    }
                };
        }

        
        public async Task<Result<Booking>> GetAgentsBooking(string referenceCode, AgentContext agentContext)
        {
            return await _bookingRecordManager.Get(referenceCode)
                .CheckPermissions(agentContext);
        }


        public async Task<Result<AccommodationBookingInfo>> GetAgentAccommodationBookingInfo(int bookingId, AgentContext agentContext, string languageCode)
        {
            var bookingDataResult = await _bookingRecordManager.Get(bookingId)
                .CheckPermissions(agentContext);
            
            if (bookingDataResult.IsFailure)
                return Result.Failure<AccommodationBookingInfo>(bookingDataResult.Error);

            var (_, isFailure, bookingInfo, error) = await ConvertToBookingInfo(bookingDataResult.Value, languageCode, agentContext);
            if (isFailure)
                return Result.Failure<AccommodationBookingInfo>(error);

            return bookingInfo;
        }


        public async Task<Result<AccommodationBookingInfo>> GetAgentAccommodationBookingInfo(string referenceCode, AgentContext agentContext, string languageCode)
        {
            var bookingDataResult = await _bookingRecordManager.Get(referenceCode)
                .CheckPermissions(agentContext);
            
            
            if (bookingDataResult.IsFailure)
                return Result.Failure<AccommodationBookingInfo>(bookingDataResult.Error);
            
            var (_, isFailure, bookingInfo, error) = await ConvertToBookingInfo(bookingDataResult.Value, languageCode, agentContext);
            if (isFailure)
                return Result.Failure<AccommodationBookingInfo>(error);

            return bookingInfo;
        }

        
        public async Task<Result<AccommodationBookingInfo>> GetAccommodationBookingInfo(string referenceCode, string languageCode)
        {
            var bookingDataResult = await _bookingRecordManager.Get(referenceCode);
            if (bookingDataResult.IsFailure)
                return Result.Failure<AccommodationBookingInfo>(bookingDataResult.Error);
            
            var (_, isFailure, bookingInfo, error) = await ConvertToBookingInfo(bookingDataResult.Value, languageCode);
            if (isFailure)
                return Result.Failure<AccommodationBookingInfo>(error);

            return bookingInfo;
        }

        
        /// <summary>
        /// Gets all booking info of the current agent
        /// </summary>
        /// <returns>List of the slim booking models </returns>
        public IQueryable<SlimAccommodationBookingInfo> GetAgentBookingsInfo(AgentContext agentContext)
        {
            var bookingData = _context.Bookings
                .Where(b => b.AgentId == agentContext.AgentId)
                .Where(b => 
                    (b.PaymentMethod == PaymentMethods.BankTransfer)
                    || (b.PaymentMethod != PaymentMethods.BankTransfer && b.PaymentStatus != BookingPaymentStatuses.NotPaid))
                .Select(b =>
                    new SlimAccommodationBookingInfo
                    {
                        Id = b.Id,
                        ReferenceCode = b.ReferenceCode,
                        AccommodationName = b.AccommodationName,
                        CountryName = b.Location.Country,
                        LocalityName = b.Location.Locality,
                        Deadline = b.DeadlineDate,
                        Price = new MoneyAmount(b.TotalPrice, b.Currency),
                        CheckInDate = b.CheckInDate,
                        CheckOutDate = b.CheckOutDate,
                        Status = b.Status,
                        PaymentStatus = b.PaymentStatus,
                        Rooms = b.Rooms,
                        Supplier = b.Supplier
                    }
                );

            return bookingData;
        }

        
        private async Task<Result<AccommodationBookingInfo>> ConvertToBookingInfo(Booking booking, string languageCode, AgentContext? agentContext = null)
        {
            var (_, isFailure, accommodation, error) = await _accommodationService.Get(booking.Supplier, booking.AccommodationId, languageCode);
            if (isFailure)
                return Result.Failure<AccommodationBookingInfo>(error.Detail);
            
            var bookingDetails = GetDetails(booking, accommodation);
            var supplier = await GetSupplier(booking, agentContext);
            var agentInformation = await GetAgentInformation(booking.AgentId, booking.AgencyId);

            return new AccommodationBookingInfo(booking.Id,
                bookingDetails,
                booking.CounterpartyId,
                booking.PaymentStatus,
                new MoneyAmount(booking.TotalPrice, booking.Currency),
                supplier,
                agentInformation,
                booking.PaymentMethod);


            static AccommodationBookingDetails GetDetails(Booking booking, Accommodation accommodationDetails)
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
            
            
            async Task<Suppliers?> GetSupplier(Booking booking, AgentContext? agent)
            {
                if (agent == null)
                    return booking.Supplier;
            
                var settings = await _accommodationBookingSettingsService.Get(agent.Value);
                return settings.IsSupplierVisible
                    ? booking.Supplier
                    : (Suppliers?) null;
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
        
        
        private readonly EdoContext _context;
        private readonly IBookingRecordManager _bookingRecordManager;
        private readonly IAccommodationService _accommodationService;
        private readonly IAccommodationBookingSettingsService _accommodationBookingSettingsService;
    }
}