using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Services.Accommodations.Availability;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Mapping;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Payments;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.Money.Models;
using HappyTravel.SuppliersCatalog;
using Microsoft.EntityFrameworkCore;
using Booking = HappyTravel.Edo.Data.Bookings.Booking;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management
{
    public class BookingInfoService : IBookingInfoService
    {
        public BookingInfoService(EdoContext context, 
            IBookingRecordManager bookingRecordManager,
            IAccommodationMapperClient accommodationMapperClient,
            IAccommodationBookingSettingsService accommodationBookingSettingsService,
            IDateTimeProvider dateTimeProvider)
        {
            _context = context;
            _bookingRecordManager = bookingRecordManager;
            _accommodationMapperClient = accommodationMapperClient;
            _accommodationBookingSettingsService = accommodationBookingSettingsService;
            _dateTimeProvider = dateTimeProvider;
        }
        
        
        public IQueryable<AgentBoundedData<SlimAccommodationBookingInfo>> GetAgencyBookingsInfo(AgentContext agentContext)
        {
            return from booking in _context.Bookings
                join agent in _context.Agents on booking.AgentId equals agent.Id
                where booking.AgencyId == agentContext.AgencyId && !BookingStatusesToHide.Contains(booking.Status)
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
                .Where(b => !BookingStatusesToHide.Contains(b.Status))
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
                        Supplier = b.Supplier,
                        Created = b.Created
                    }
                );

            return bookingData;
        }

        
        public Task<List<BookingStatusHistoryEntry>> GetBookingStatusHistory(int bookingId) 
            => _context.BookingStatusHistory
                .Where(bsh => bsh.BookingId == bookingId)
                .OrderBy(bsh => bsh.Id)
                .ToListAsync();


        public async Task<Result<List<BookingConfirmationHistoryEntry>>> GetBookingConfirmationHistory(string referenceCode)
        {
            var history = await _context.BookingConfirmationHistory
                .Where(bch => bch.ReferenceCode == referenceCode)
                .OrderBy(bch => bch.Id)
                .ToListAsync();

            return history ?? emptyBookingConfirmationHistory;
        }


        private async Task<Result<AccommodationBookingInfo>> ConvertToBookingInfo(Booking booking, string languageCode, AgentContext? agentContext = null)
        {
            var (_, isFailure, accommodation, error) = await _accommodationMapperClient.GetAccommodation(booking.HtId, languageCode);
            if (isFailure)
                return Result.Failure<AccommodationBookingInfo>(error.Detail);

            var settings = agentContext.HasValue
                ? await _accommodationBookingSettingsService.Get(agentContext.Value)
                : (AccommodationBookingSettings?) null;
            
            var bookingDetails = GetDetails(booking, accommodation.ToEdoContract());
            var supplier = GetSupplier(booking, settings);
            var isDirectContract = GetDirectContractFlag(booking, settings);
            var agentInformation = await GetAgentInformation(booking.AgentId, booking.AgencyId);
            
            return new AccommodationBookingInfo(
                bookingId: booking.Id,
                bookingDetails: bookingDetails,
                agencyId: booking.AgencyId,
                paymentStatus: booking.PaymentStatus,
                totalPrice: new MoneyAmount(booking.TotalPrice, booking.Currency),
                cancellationPenalty: BookingCancellationPenaltyCalculator.Calculate(booking, _dateTimeProvider.UtcNow()),
                supplier: supplier,
                agentInformation: agentInformation,
                paymentMethod: booking.PaymentType,
                tags: booking.Tags,
                isDirectContract: isDirectContract);


            static AccommodationBookingDetails GetDetails(Booking booking, Accommodation accommodationDetails)
            {
                var passengerNumber = booking.Rooms.Sum(r => r.Passengers.Count);
                var numberOfNights = (booking.CheckOutDate - booking.CheckInDate).Days;
                return new AccommodationBookingDetails(
                    referenceCode: booking.ReferenceCode,
                    clientReferenceCode: booking.ClientReferenceCode,
                    agentReference: booking.SupplierReferenceCode,
                    status: booking.Status,
                    numberOfNights: numberOfNights,
                    checkInDate: booking.CheckInDate,
                    checkOutDate: booking.CheckOutDate,
                    location: booking.Location,
                    contactInfo: accommodationDetails.Contacts,
                    accommodationId: booking.AccommodationId,
                    accommodationName: booking.AccommodationName,
                    accommodationInfo: booking.AccommodationInfo,
                    deadlineDate: booking.DeadlineDate,
                    roomDetails: booking.Rooms,
                    numberOfPassengers: passengerNumber,
                    cancellationPolicies: booking.CancellationPolicies,
                    created: booking.Created,
                    propertyOwnerConfirmationCode: booking.PropertyOwnerConfirmationCode,
                    isAdvancePurchaseRate: booking.IsAdvancePurchaseRate);
            }
            
            
            static Suppliers? GetSupplier(Booking booking, AccommodationBookingSettings? settings)
                => settings switch
                {
                    null => booking.Supplier,
                    {IsSupplierVisible: true} => booking.Supplier,
                    _ => null
                };
            
            
            static bool? GetDirectContractFlag(Booking booking, AccommodationBookingSettings? settings)
                => settings switch
                {
                    {IsDirectContractFlagVisible: true} => booking.IsDirectContract,
                    _ => null
                };


            Task<AccommodationBookingInfo.BookingAgentInformation> GetAgentInformation(int agentId, int agencyId)
            {
                var agencyInfoQuery = from agent in _context.Agents
                    join relation in _context.AgentAgencyRelations on agent.Id equals relation.AgentId
                    join agency in _context.Agencies on relation.AgencyId equals agency.Id
                    where agent.Id == booking.AgentId && agency.Id == booking.AgencyId
                    let agentName = $"{agent.FirstName} {agent.LastName}"
                    select new AccommodationBookingInfo.BookingAgentInformation(agentName,
                        agency.Name, agent.Email);

                return agencyInfoQuery.SingleOrDefaultAsync();
            }
        }


        private static readonly HashSet<BookingStatuses> BookingStatusesToHide = new()
        {
            BookingStatuses.Created,
            BookingStatuses.Invalid
        };
        private static readonly List<BookingConfirmationHistoryEntry> emptyBookingConfirmationHistory = new(0);

        private readonly EdoContext _context;
        private readonly IBookingRecordManager _bookingRecordManager;
        private readonly IAccommodationMapperClient _accommodationMapperClient;
        private readonly IAccommodationBookingSettingsService _accommodationBookingSettingsService;
        private readonly IDateTimeProvider _dateTimeProvider;
    }
}