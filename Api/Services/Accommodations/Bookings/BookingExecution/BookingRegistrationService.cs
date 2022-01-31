using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Payments;
using HappyTravel.Edo.Api.Services.CodeProcessors;
using HappyTravel.Edo.Api.Services.SupplierOrders;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Bookings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.BookingExecution
{
    public class BookingRegistrationService : IBookingRegistrationService
    {
        public BookingRegistrationService(EdoContext context, ITagProcessor tagProcessor, IDateTimeProvider dateTimeProvider,
            IAppliedBookingMarkupRecordsManager appliedBookingMarkupRecordsManager, IBookingChangeLogService changeLogService,
            ISupplierOrderService supplierOrderService, IBookingRequestStorage requestStorage,
            ILogger<BookingRegistrationService> logger)
        {
            _context = context;
            _tagProcessor = tagProcessor;
            _dateTimeProvider = dateTimeProvider;
            _appliedBookingMarkupRecordsManager = appliedBookingMarkupRecordsManager;
            _changeLogService = changeLogService;
            _supplierOrderService = supplierOrderService;
            _requestStorage = requestStorage;
            _logger = logger;
        }
        
        
        public async Task<Result<Booking>> Register(AccommodationBookingRequest bookingRequest,
            BookingAvailabilityInfo availabilityInfo, PaymentTypes paymentMethod, AgentContext agentContext, string languageCode)
        {
            var (_, isFailure, booking, error) = await CheckRooms()
                .Map(GetTags)
                .Map(Create)
                .Tap(SaveRequestInfo)
                .Tap(LogBookingStatus)
                .Tap(SaveMarkups)
                .Tap(CreateSupplierOrder);

            if (isFailure)
                return Result.Failure<Booking>(error);

            _logger.LogBookingRegistrationSuccess(booking.ReferenceCode);
            return booking;


            Result CheckRooms()
            {
                if (bookingRequest.RoomDetails.Count != availabilityInfo.AvailabilityRequest.RoomDetails.Count)
                    return Result.Failure("Rooms does not correspond to search rooms");

                for (var i = 0; i < bookingRequest.RoomDetails.Count; i++)
                {
                    var adultsCount = bookingRequest.RoomDetails[i].Passengers.Count(p => p.Age == null);
                    var childrenCount = bookingRequest.RoomDetails[i].Passengers.Count(p => p.Age != null);
                    
                    if (availabilityInfo.AvailabilityRequest.RoomDetails[i].AdultsNumber != adultsCount ||
                        availabilityInfo.AvailabilityRequest.RoomDetails[i].ChildrenAges.Count != childrenCount)
                        return Result.Failure("Rooms does not correspond to search rooms");
                }

                return Result.Success();
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

                    if (!await AreExistBookingsForItn(itn, agentContext.AgentId))
                        itn = await _tagProcessor.GenerateItn();
                }

                var referenceCode = await _tagProcessor.GenerateReferenceCode(
                    ServiceTypes.HTL,
                    availabilityInfo.CountryCode,
                    itn);

                return (itn, referenceCode);
            }


            async Task<Booking> Create((string itn, string referenceCode) tags)
            {
                var createdBooking = await CreateBooking(
                    created: _dateTimeProvider.UtcNow(),
                    agentContext: agentContext,
                    itineraryNumber: tags.itn,
                    referenceCode: tags.referenceCode,
                    clientReferenceCode: bookingRequest.ClientReferenceCode,
                    availabilityInfo: availabilityInfo,
                    paymentMethod: paymentMethod,
                    bookingRequest: bookingRequest,
                    languageCode: languageCode);

                _context.Bookings.Add(createdBooking);
                await _context.SaveChangesAsync();
                _context.Entry(createdBooking).State = EntityState.Detached;

                return createdBooking;
            }


            Task SaveRequestInfo(Booking booking) 
                => _requestStorage.Set(booking.ReferenceCode, bookingRequest, availabilityInfo);


            Task LogBookingStatus(Booking booking)
            {
                var changeReason = new BookingChangeReason
                {
                    Event = BookingChangeEvents.Create,
                    Source = BookingChangeSources.System
                };
                return _changeLogService.Write(booking, BookingStatuses.Created, booking.Created.DateTime, 
                    agentContext.ToApiCaller(), changeReason);
            }


            Task SaveMarkups(Booking booking) 
                => _appliedBookingMarkupRecordsManager.Create(booking.ReferenceCode, availabilityInfo.AppliedMarkups);


            Task CreateSupplierOrder(Booking booking) 
                => _supplierOrderService.Add(referenceCode: booking.ReferenceCode,
                    serviceType: ServiceTypes.HTL, 
                    convertedPrice: availabilityInfo.ConvertedSupplierPrice, 
                    supplierPrice: availabilityInfo.OriginalSupplierPrice, 
                    deadline: availabilityInfo.SupplierDeadline, 
                    supplier: (int) booking.Supplier,
                    paymentType: availabilityInfo.CardRequirement is not null
                        ? SupplierPaymentType.CreditCard
                        : SupplierPaymentType.DirectPayment,
                    paymentDate: availabilityInfo.RoomContractSet.IsAdvancePurchaseRate
                        ? booking.Created.DateTime
                        : booking.CheckOutDate.DateTime);
        }


        private async Task<Booking> CreateBooking(DateTime created, AgentContext agentContext, string itineraryNumber,
            string referenceCode, string clientReferenceCode, BookingAvailabilityInfo availabilityInfo, PaymentTypes paymentMethod,
            AccommodationBookingRequest bookingRequest, string languageCode)
        {
            var booking = new Booking
            {
                Created = created,
                ItineraryNumber = itineraryNumber,
                ReferenceCode = referenceCode,
                ClientReferenceCode = clientReferenceCode,
                Status = BookingStatuses.Created,
                PaymentType = paymentMethod,
                LanguageCode = languageCode,
                Supplier = availabilityInfo.SupplierId,
                PaymentStatus = BookingPaymentStatuses.NotPaid,
                DeadlineDate = availabilityInfo.RoomContractSet.Deadline.Date,
                CheckInDate = availabilityInfo.CheckInDate,
                CheckOutDate = availabilityInfo.CheckOutDate,
                HtId = availabilityInfo.HtId,
                Tags = availabilityInfo.RoomContractSet.Tags,
                IsDirectContract = availabilityInfo.RoomContractSet.IsDirectContract,
                CancellationPolicies = availabilityInfo.RoomContractSet.Deadline.Policies,
                IsAdvancePurchaseRate = availabilityInfo.RoomContractSet.IsAdvancePurchaseRate,
                IsPackage = availabilityInfo.RoomContractSet.IsPackageRate
            };
            
            AddRequestInfo(bookingRequest);
            AddServiceDetails();
            AddAgentInfo();
            AddRooms(availabilityInfo.RoomContractSet.Rooms, bookingRequest.RoomDetails);
            booking = await AddStaticData(booking, availabilityInfo);

            return booking;


            void AddRequestInfo(in AccommodationBookingRequest bookingRequestInternal)
            {
                booking.Nationality = availabilityInfo.AvailabilityRequest.Nationality.ToUpper();
                booking.Residency = availabilityInfo.AvailabilityRequest.Residency.ToUpper();
                booking.MainPassengerName = bookingRequestInternal.MainPassengerName.Trim();
            }

            void AddServiceDetails()
            {
                var rate = availabilityInfo.RoomContractSet.Rate;
                booking.TotalPrice = rate.FinalPrice.Amount;
                booking.Currency = rate.Currency;
            }

            void AddAgentInfo()
            {
                booking.AgentId = agentContext.AgentId;
                booking.AgencyId = agentContext.AgencyId;
            }
            
            void AddRooms(List<RoomContract> roomContracts, List<BookingRoomDetails> bookingRequestRoomDetails)
            {
                booking.Rooms = roomContracts
                    .Select((r, number) =>
                        new BookedRoom(r.Type,
                            r.IsExtraBedNeeded,
                            r.Rate.FinalPrice, 
                            r.BoardBasis,
                            r.MealPlan,
                            r.Deadline.Date,
                            r.ContractDescription,
                            r.Remarks,
                            r.Deadline,
                            GetCorrespondingPassengers(number),
                            r.IsAdvancePurchaseRate,
                            string.Empty))
                    .ToList();


                List<Passenger> GetCorrespondingPassengers(int number)
                    => bookingRequestRoomDetails[number].Passengers
                        .Select(p => new Passenger(p.Title, p.LastName.Trim(), p.FirstName.Trim(), p.IsLeader, p.Age))
                        .ToList();
            }
        }


        protected virtual Task<Booking> AddStaticData(Booking booking, BookingAvailabilityInfo availabilityInfo)
        {
            booking.Location = new AccommodationLocation(availabilityInfo.CountryName,
                availabilityInfo.LocalityName,
                availabilityInfo.ZoneName,
                availabilityInfo.Address,
                availabilityInfo.Coordinates);

            booking.AccommodationId = availabilityInfo.AccommodationId;
            booking.AccommodationName = availabilityInfo.AccommodationName ?? string.Empty;
            booking.AccommodationInfo = new Data.Bookings.AccommodationInfo(
                new ImageInfo(availabilityInfo.AccommodationInfo.Photo.Caption, availabilityInfo.AccommodationInfo.Photo.SourceUrl));

            return Task.FromResult(booking);
        }


        // TODO: Replace method when will be added other services 
        private Task<bool> AreExistBookingsForItn(string itn, int agentId)
            => _context.Bookings.Where(b => b.AgentId == agentId && b.ItineraryNumber == itn).AnyAsync();

        private static readonly TimeSpan BookingLockDuration = TimeSpan.FromMinutes(10);

        
        private readonly EdoContext _context;
        private readonly ITagProcessor _tagProcessor;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IAppliedBookingMarkupRecordsManager _appliedBookingMarkupRecordsManager;
        private readonly IBookingChangeLogService _changeLogService;
        private readonly ISupplierOrderService _supplierOrderService;
        private readonly IBookingRequestStorage _requestStorage;
        private readonly ILogger<BookingRegistrationService> _logger;
    }
}