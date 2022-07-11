using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Services.Accommodations.Availability;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Mapping;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Payments;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Common.Enums.Administrators;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.Money.Models;
using HappyTravel.SupplierOptionsProvider;
using Microsoft.EntityFrameworkCore;
using Booking = HappyTravel.Edo.Data.Bookings.Booking;
using BookingStatusHistoryEntry = HappyTravel.Edo.Api.Models.Bookings.BookingStatusHistoryEntry;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management
{
    public class BookingInfoService : IBookingInfoService
    {
        public BookingInfoService(EdoContext context,
            IBookingRecordManager bookingRecordManager,
            IAccommodationMapperClient accommodationMapperClient,
            IAccommodationBookingSettingsService accommodationBookingSettingsService,
            ISupplierOptionsStorage supplierOptionsStorage,
            IDateTimeProvider dateTimeProvider,
            IAgentContextService agentContextService,
            IAdministratorContext adminContext)
        {
            _context = context;
            _bookingRecordManager = bookingRecordManager;
            _accommodationMapperClient = accommodationMapperClient;
            _accommodationBookingSettingsService = accommodationBookingSettingsService;
            _supplierOptionsStorage = supplierOptionsStorage;
            _dateTimeProvider = dateTimeProvider;
            _agentContextService = agentContextService;
            _adminContext = adminContext;
        }


        public IQueryable<AgentBoundedData<SlimAccommodationBookingInfo>> GetAgencyBookingsInfo(AgentContext agentContext)
        {
            var suppliersDictionary = GetSuppliersDictionary();

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
                        ClientReferenceCode = booking.ClientReferenceCode,
                        AccommodationName = booking.AccommodationName,
                        CountryName = booking.Location.Country,
                        LocalityName = booking.Location.Locality,
                        Deadline = booking.DeadlineDate,
                        Price = new MoneyAmount(booking.TotalPrice, booking.Currency),
                        CheckInDate = booking.CheckInDate.DateTime,
                        CheckOutDate = booking.CheckOutDate.DateTime,
                        Status = booking.Status,
                        PaymentStatus = booking.PaymentStatus,
                        Rooms = booking.Rooms,
                        Supplier = suppliersDictionary[booking.SupplierCode]
                    }
                };
        }


        public async Task<Result<Booking>> GetAgentsBooking(string referenceCode)
        {
            var agent = await _agentContextService.GetAgent();
            return await _bookingRecordManager.Get(referenceCode)
                .CheckPermissions(agent);
        }


        public async Task<Result<AccommodationBookingInfo>> GetAgentAccommodationBookingInfo(int bookingId, string languageCode)
        {
            var agent = await _agentContextService.GetAgent();
            return await _bookingRecordManager.Get(bookingId)
                .CheckPermissions(agent)
                .Bind(booking => ConvertToBookingInfo(booking, languageCode, agent));
        }


        public async Task<Result<AccommodationBookingInfo>> GetAgentAccommodationBookingInfo(string referenceCode, string languageCode)
        {
            var agent = await _agentContextService.GetAgent();
            return await _bookingRecordManager.Get(referenceCode)
                .CheckPermissions(agent)
                .Bind(booking => ConvertToBookingInfo(booking, languageCode, agent));
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


        public async Task<Result<AccommodationBookingInfo>> GetAdministratorAccommodationBookingInfo(string referenceCode, string languageCode)
        {
            var accommodationBookingInfo = await GetAccommodationBookingInfo(referenceCode, languageCode);
            if (accommodationBookingInfo.IsFailure)
                return Result.Failure<AccommodationBookingInfo>(accommodationBookingInfo.Error);

            var (_, isFailure, bookingInfo, error) = await MaskPaxNamesIfNeeded(accommodationBookingInfo);
            if (isFailure)
                return Result.Failure<AccommodationBookingInfo>(error);

            return bookingInfo;
        }


        private async Task<Result<AccommodationBookingInfo>> MaskPaxNamesIfNeeded(Result<AccommodationBookingInfo> accommodationBookingInfo)
        {
            if (await _adminContext.HasPermission(AdministratorPermissions.ViewPaxNames))
                return accommodationBookingInfo;

            foreach (var passenger in accommodationBookingInfo.Value.BookingDetails.RoomDetails.SelectMany(roomDetail => roomDetail.Passengers))
            {
                passenger.FirstName = "***";
                passenger.LastName = "***";
            }
            return accommodationBookingInfo;
        }


        /// <summary>
        /// Gets all booking info of the current agent
        /// </summary>
        /// <returns>List of the slim booking models </returns>
        public IQueryable<SlimAccommodationBookingInfo> GetAgentBookingsInfo(AgentContext agentContext)
        {
            var suppliersDictionary = GetSuppliersDictionary();
            var bookingData = _context.Bookings
                .Where(b => b.AgentId == agentContext.AgentId)
                .Where(b => !BookingStatusesToHide.Contains(b.Status))
                .Select(b =>
                    new SlimAccommodationBookingInfo
                    {
                        Id = b.Id,
                        ReferenceCode = b.ReferenceCode,
                        ClientReferenceCode = b.ClientReferenceCode,
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
                        Supplier = suppliersDictionary[b.SupplierCode],
                        Created = b.Created
                    }
                );

            return bookingData;
        }


        public Task<List<BookingStatusHistoryEntry>> GetBookingStatusHistory(int bookingId)
        {
            var query = from entry in _context.BookingStatusHistory
                where entry.BookingId == bookingId
                orderby entry.Id
                let agent = _context.Agents.SingleOrDefault(a => a.Id.ToString() == entry.UserId && entry.Initiator == BookingChangeInitiators.Agent)
                select new BookingStatusHistoryEntry(entry.Id,
                    entry.BookingId,
                    entry.UserId,
                    entry.ApiCallerType,
                    entry.AgencyId,
                    entry.CreatedAt,
                    entry.Status,
                    entry.Initiator,
                    entry.Source,
                    entry.Event,
                    entry.Reason,
                    agent != null
                        ? $"{agent.FirstName} {agent.LastName}"
                        : null
                );

            return query.ToListAsync();
        }


        public Task<List<AdministratorBookingStatusHistoryEntry>> GetAdministratorBookingStatusHistory(int bookingId)
        {
            var query = from entry in _context.BookingStatusHistory
                where entry.BookingId == bookingId
                orderby entry.Id
                let agent = _context.Agents.SingleOrDefault(a => a.Id.ToString() == entry.UserId && entry.Initiator == BookingChangeInitiators.Agent)
                let admin = _context.Administrators.SingleOrDefault(a => a.Id.ToString() == entry.UserId && entry.Initiator == BookingChangeInitiators.Administrator)
                select new AdministratorBookingStatusHistoryEntry(entry.Id,
                    entry.BookingId,
                    entry.UserId,
                    entry.ApiCallerType,
                    entry.AgencyId,
                    entry.CreatedAt,
                    entry.Status,
                    entry.Initiator,
                    entry.Source,
                    entry.Event,
                    entry.Reason,
                    agent != null
                        ? $"{agent.FirstName} {agent.LastName}"
                        : null,
                    admin != null
                        ? $"{admin.FirstName} {admin.LastName}"
                        : null
                );

            return query.ToListAsync();
        }


        public async Task<Result<List<BookingConfirmationHistoryEntry>>> GetBookingConfirmationHistory(string referenceCode)
        {
            var history = await _context.BookingConfirmationHistory
                .Where(bch => bch.ReferenceCode == referenceCode)
                .OrderBy(bch => bch.Id)
                .ToListAsync();

            return history ?? EmptyBookingConfirmationHistory;
        }


        private Dictionary<string, string> GetSuppliersDictionary()
        {
            var (_, isFailure, suppliers, _) = _supplierOptionsStorage.GetAll();
            return isFailure
                ? new Dictionary<string, string>(0)
                : suppliers.ToDictionary(s => s.Code, s => s.Name);
        }


        public async Task<Result<AccommodationBookingInfo>> ConvertToBookingInfo(Booking booking, string languageCode, AgentContext? agentContext = null)
        {
            var settings = agentContext.HasValue
                ? await _accommodationBookingSettingsService.Get()
                : (AccommodationBookingSettings?)null;

            await LoadContactInfoIfNeed();
            var bookingDetails = GetDetails(booking);
            var supplier = GetSupplier(booking, settings);
            var isDirectContract = GetDirectContractFlag(booking, settings);
            var agentInformation = await GetAgentInformation(booking.AgentId, booking.AgencyId);

            return new AccommodationBookingInfo(
                bookingId: booking.Id,
                bookingDetails: bookingDetails,
                agencyId: booking.AgencyId,
                paymentStatus: booking.PaymentStatus,
                totalPrice: new MoneyAmount(booking.TotalPrice, booking.Currency),
                creditCardPrice: new MoneyAmount(booking.CreditCardPrice, booking.Currency),
                cancellationPenalty: BookingCancellationPenaltyCalculator.Calculate(booking, _dateTimeProvider.UtcNow()),
                supplier: supplier,
                agentInformation: agentInformation,
                paymentMethod: booking.PaymentType,
                tags: booking.Tags,
                isDirectContract: isDirectContract,
                cancellationDate: booking.Cancelled);

           async Task LoadContactInfoIfNeed()
            {
                if (booking.AccommodationInfo.ContactInfo is null)
                {
                    var (_, isFailure, accommodation, error) = await _accommodationMapperClient.GetAccommodation(booking.HtId, languageCode);
                    if (!isFailure)
                    {
                        booking.AccommodationInfo.ContactInfo = new ContactInfo(accommodation.Contacts.Emails, accommodation.Contacts.Phones, accommodation.Contacts.WebSites,
                            accommodation.Contacts.Faxes);
                        await _bookingRecordManager.Update(booking);
                    }
                       
                }
            }


            static AccommodationBookingDetails GetDetails(Booking booking)
            {
                var passengerNumber = booking.Rooms.Sum(r => r.Passengers.Count);
                var numberOfNights = (booking.CheckOutDate - booking.CheckInDate).Days;
                return new AccommodationBookingDetails(
                    referenceCode: booking.ReferenceCode,
                    clientReferenceCode: booking.ClientReferenceCode,
                    agentReference: booking.SupplierReferenceCode,
                    status: booking.Status,
                    numberOfNights: numberOfNights,
                    checkInDate: booking.CheckInDate.DateTime,
                    checkOutDate: booking.CheckOutDate.DateTime,
                    location: booking.Location,
                    contactInfo: booking.AccommodationInfo.ContactInfo,
                    accommodationId: booking.AccommodationId,
                    accommodationName: booking.AccommodationName,
                    accommodationInfo: booking.AccommodationInfo,
                    deadlineDate: booking.DeadlineDate?.DateTime,
                    roomDetails: booking.Rooms,
                    numberOfPassengers: passengerNumber,
                    cancellationPolicies: booking.CancellationPolicies,
                    created: booking.Created.DateTime,
                    propertyOwnerConfirmationCode: booking.PropertyOwnerConfirmationCode,
                    isAdvancePurchaseRate: booking.IsAdvancePurchaseRate);
            }


            string? GetSupplier(Booking booking, AccommodationBookingSettings? settings)
            {
                return settings switch
                {
                    null => GetName(booking.SupplierCode),
                    { IsSupplierVisible: true } => GetName(booking.SupplierCode),
                    _ => null
                };


                string GetName(string code)
                {
                    var (_, isFailure, supplier, _) = _supplierOptionsStorage.Get(code);
                    return isFailure
                        ? null
                        : supplier.Name;
                }
            }


            static bool? GetDirectContractFlag(Booking booking, AccommodationBookingSettings? settings)
                => settings switch
                {
                    { IsDirectContractFlagVisible: true } => booking.IsDirectContract,
                    _ => null
                };


            Task<BookingAgentInformation> GetAgentInformation(int agentId, int agencyId)
            {
                var agencyInfoQuery = from agent in _context.Agents
                    join relation in _context.AgentAgencyRelations on agent.Id equals relation.AgentId
                    join agency in _context.Agencies on relation.AgencyId equals agency.Id
                    where agent.Id == booking.AgentId && agency.Id == booking.AgencyId
                    let agentName = $"{agent.FirstName} {agent.LastName}"
                    select new BookingAgentInformation(agentName,
                        agency.Name, agent.Email);

                return agencyInfoQuery.SingleOrDefaultAsync();
            }
        }


        private static readonly HashSet<BookingStatuses> BookingStatusesToHide = new()
        {
            BookingStatuses.Created,
            BookingStatuses.Invalid
        };


        private static readonly List<BookingConfirmationHistoryEntry> EmptyBookingConfirmationHistory = new(0);


        private readonly EdoContext _context;
        private readonly IBookingRecordManager _bookingRecordManager;
        private readonly IAccommodationMapperClient _accommodationMapperClient;
        private readonly IAccommodationBookingSettingsService _accommodationBookingSettingsService;
        private readonly ISupplierOptionsStorage _supplierOptionsStorage;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IAgentContextService _agentContextService;
        private readonly IAdministratorContext _adminContext;
    }
}