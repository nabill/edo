using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Services.CodeGeneration;
using HappyTravel.Edo.Api.Services.Customers;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Booking;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Enums;
using HappyTravel.EdoContracts.Accommodations.Internals;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PaymentMethods = HappyTravel.EdoContracts.General.Enums.PaymentMethods;

namespace HappyTravel.Edo.Api.Services.Accommodations
{
    internal class AccommodationBookingManager : IAccommodationBookingManager
    {
        public AccommodationBookingManager(IOptions<DataProviderOptions> options,
            IDataProviderClient dataProviderClient,
            EdoContext context,
            IDateTimeProvider dateTimeProvider,
            ICustomerContext customerContext,
            ITagGenerator tagGenerator)
        {
            _dataProviderClient = dataProviderClient;
            _options = options.Value;
            _context = context;
            _dateTimeProvider = dateTimeProvider;
            _customerContext = customerContext;
            _tagGenerator = tagGenerator;
        }


        public async Task<Result<BookingDetails, ProblemDetails>> Book(AccommodationBookingRequest bookingRequest,
            BookingAvailabilityInfo availability, string languageCode)
        {
            var (_, isFailure, customerInfo, error) = await _customerContext.GetCustomerInfo();

            if (isFailure)
                return ProblemDetailsBuilder.Fail<BookingDetails>(error);

            var itn = !string.IsNullOrWhiteSpace(bookingRequest.ItineraryNumber)
                ? bookingRequest.ItineraryNumber
                : await _tagGenerator.GenerateItn();

            var referenceCode = await _tagGenerator.GenerateReferenceCode(ServiceTypes.HTL,
                availability.CountryCode,
                itn);

            return await ExecuteBookingRequest()
                .OnSuccess(SaveBookingResult);


            Task<Result<BookingDetails, ProblemDetails>> ExecuteBookingRequest()
            {
                // TODO: will be implemented in NIJO-31 
                var features = new List<Feature>(); //bookingRequest.Features
                
                var roomDetails = new List<SlimRoomDetails>(); // bookingRequest.RoomDetails

                var innerRequest = new BookingRequest(availability.AccommodationId, bookingRequest.AvailabilityId, bookingRequest.AgreementId,
                    availability.CheckInDate, availability.CheckOutDate, bookingRequest.Nationality, PaymentMethods.BankTransfer, referenceCode,
                    bookingRequest.Residency, availability.Agreement.TariffCode, roomDetails, features, bookingRequest.RejectIfUnavailable);

                return _dataProviderClient.Post<BookingRequest, BookingDetails>(
                    new Uri(_options.Netstorming + "bookings/accommodations", UriKind.Absolute),
                    innerRequest, languageCode);
            }


            Task SaveBookingResult(BookingDetails confirmedBooking)
            {
                var booking = new AccommodationBookingBuilder()
                    .AddCustomerInfo(customerInfo)
                    .AddTags(itn, referenceCode)
                    .AddRequestInfo(bookingRequest)
                    .AddConfirmationDetails(confirmedBooking)
                    .AddServiceDetails(availability)
                    .AddCreationDate(_dateTimeProvider.UtcNow())
                    .Build();

                _context.Bookings.Add(booking);
                return _context.SaveChangesAsync();
            }
        }


        public async Task<Result<AccommodationBookingInfo>> Get(int bookingId)
        {
            var (_, isFailure, customerData, error) = await _customerContext.GetCustomerInfo();

            if (isFailure)
                return ProblemDetailsBuilder.Fail<AccommodationBookingInfo>(error);

            var bookingData = await _context.Bookings
                .Where(b => b.CustomerId == customerData.CustomerId)
                .Where(b => b.Id == bookingId)
                .Select(b => new AccommodationBookingInfo(b.Id, b.BookingDetails, b.ServiceDetails, b.CompanyId))
                .FirstOrDefaultAsync();

            return bookingData.Equals(default)
                ? Result.Fail<AccommodationBookingInfo>("Could not get a booking data")
                : Result.Ok(bookingData);
        }


        public async Task<Result<List<SlimAccommodationBookingInfo>>> GetForCurrentCustomer()
        {
            var (_, isFailure, customerData, error) = await _customerContext.GetCustomerInfo();

            if (isFailure)
                return ProblemDetailsBuilder.Fail<List<SlimAccommodationBookingInfo>>(error);

            var bookingData = await _context.Bookings
                .Where(b => b.CustomerId == customerData.CustomerId)
                .Select(b =>
                    new SlimAccommodationBookingInfo(b)
                ).ToListAsync();

            return Result.Ok(bookingData);
        }


        public async Task<Result<Booking, ProblemDetails>> Cancel(int bookingId)
        {
            var (_, isFailure, customerData, error) = await _customerContext.GetCustomerInfo();
            if (isFailure)
                return ProblemDetailsBuilder.Fail<Booking>(error);

            var booking = await _context.Bookings
                .SingleOrDefaultAsync(b => b.Id == bookingId && b.CustomerId == customerData.CustomerId);

            if (booking is null)
                return ProblemDetailsBuilder.Fail<Booking>($"Could not find booking with id '{bookingId}'");

            if (booking.Status == BookingStatusCodes.Cancelled)
                return ProblemDetailsBuilder.Fail<Booking>("Booking was already cancelled");

            return await ExecuteBookingCancel()
                .OnSuccess(async voidObj => await ChangeBookingToCancelled(booking));


            Task<Result<VoidObject, ProblemDetails>> ExecuteBookingCancel()
                => _dataProviderClient.Post(new Uri(_options.Netstorming + "hotels/booking/" + booking.ReferenceCode + "/cancel",
                    UriKind.Absolute));


            async Task<Booking> ChangeBookingToCancelled(Booking bookingToCancel)
            {
                bookingToCancel.Status = BookingStatusCodes.Cancelled;
                if (booking.PaymentStatus == BookingPaymentStatuses.MoneyFrozen)
                    booking.PaymentStatus = BookingPaymentStatuses.Cancelled;
                var currentDetails = JsonConvert.DeserializeObject<AccommodationBookingDetails>(bookingToCancel.BookingDetails);
                bookingToCancel.BookingDetails = JsonConvert.SerializeObject(new AccommodationBookingDetails(currentDetails, BookingStatusCodes.Cancelled));

                _context.Update(bookingToCancel);
                await _context.SaveChangesAsync();
                return bookingToCancel;
            }
        }


        private readonly EdoContext _context;
        private readonly ICustomerContext _customerContext;
        private readonly IDataProviderClient _dataProviderClient;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly DataProviderOptions _options;
        private readonly ITagGenerator _tagGenerator;
        private readonly IPermissionChecker _permissionChecker;
    }
}