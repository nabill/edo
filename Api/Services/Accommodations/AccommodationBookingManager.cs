using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Users;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Services.CodeProcessors;
using HappyTravel.Edo.Api.Services.Customers;
using HappyTravel.Edo.Api.Services.Management;
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
            IServiceAccountContext serviceAccountContext,
            ITagProcessor tagProcessor)
        {
            _dataProviderClient = dataProviderClient;
            _options = options.Value;
            _context = context;
            _dateTimeProvider = dateTimeProvider;
            _customerContext = customerContext;
            _serviceAccountContext = serviceAccountContext;
            _tagProcessor = tagProcessor;
        }


        public async Task<Result<BookingDetails, ProblemDetails>> SendBookingRequest(AccommodationBookingRequest bookingRequest,
            BookingAvailabilityInfo availabilityInfo,
            string languageCode)
        {
            var (_, isCustomerFailure, customerInfo, customerError) = await _customerContext.GetCustomerInfo();

            if (isCustomerFailure)
                return ProblemDetailsBuilder.Fail<BookingDetails>(customerError);

            var tags = await GetTags();
            return await ExecuteBookingRequest()
                .OnSuccess(AddNewBooking);
            
            
            Task<Result<BookingDetails, ProblemDetails>>  ExecuteBookingRequest()
            {
                // TODO: will be implemented in NIJO-31 
                var features = new List<Feature>(); //bookingRequest.Features

                var roomDetails = bookingRequest.RoomDetails.Select(d => new SlimRoomDetails(d.Type, d.Passengers, d.IsExtraBedNeeded)).ToList();

                var request = new BookingRequest(bookingRequest.AvailabilityId, bookingRequest.AgreementId, bookingRequest.Nationality,
                    PaymentMethods.BankTransfer, tags.referenceCode, bookingRequest.Residency, roomDetails, features, bookingRequest.RejectIfUnavailable);

                return _dataProviderClient.Post<BookingRequest, BookingDetails>(new Uri(_options.Netstorming + "bookings/accommodations", UriKind.Absolute),
                    request, languageCode);
            }


            async Task<BookingDetails> AddNewBooking(BookingDetails newBookingDetails)
            {
                //Status is WaitingForResponse
                var booking = new AccommodationBookingBuilder().AddCustomerInfo(customerInfo)
                    .AddTags(tags.itn, tags.referenceCode)
                    .AddCreationDate(_dateTimeProvider.UtcNow())
                    .AddStatus(newBookingDetails.Status)
                    .AddRequestInfo(bookingRequest)
                    .AddServiceDetails(availabilityInfo)
                    .Build();
                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();
                return newBookingDetails;
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

                    if (!await AreExistBookingsForItn(itn, customerInfo.CompanyId))
                        itn = await _tagProcessor.GenerateItn();
                }

                var referenceCode = await _tagProcessor.GenerateReferenceCode(ServiceTypes.HTL, availabilityInfo.CountryCode, itn);

                return (itn, referenceCode);
            }
        }


        public async Task<Result<Booking>> UpdateBookingDetails(Booking booking, BookingDetails bookingDetails)
        {
            booking.BookingDetails = JsonConvert.SerializeObject(bookingDetails);
            booking.Status = bookingDetails.Status;
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();

            return Result.Ok(booking);
        }


        public async Task<Result<AccommodationBookingInfo>> GetCustomerBooking(int bookingId)
        {
            var (_, isCustomerFailure, customerData, customerError) = await _customerContext.GetCustomerInfo();
            if (isCustomerFailure)
                return Result.Fail<AccommodationBookingInfo>(customerError);

            var bookingDataResult = await GetRaw(booking => customerData.CustomerId == booking.CustomerId && booking.Id == bookingId);
            if (bookingDataResult.IsFailure)
                return Result.Fail<AccommodationBookingInfo>(bookingDataResult.Error);
            
            return Result.Ok(ConvertToBookingInfo(bookingDataResult.Value));
        }
       

        public async Task<Result<AccommodationBookingInfo>> GetCustomerBooking(string referenceCode)
        {
            var (_, isCustomerFailure, customerData, customerError) = await _customerContext.GetCustomerInfo();
            if (isCustomerFailure)
                return Result.Fail<AccommodationBookingInfo>(customerError);
            
            var bookingDataResult = await GetRaw(booking => customerData.CustomerId == booking.CustomerId && booking.ReferenceCode == referenceCode);
            if (bookingDataResult.IsFailure)
                return Result.Fail<AccommodationBookingInfo>(bookingDataResult.Error);
            
            return Result.Ok(ConvertToBookingInfo(bookingDataResult.Value));
        }


        public async Task<Result<Booking>> GetRawCustomerBooking(int bookingId)
        {
            var (_, isCustomerFailure, customerData, customerError) = await _customerContext.GetCustomerInfo();
            if (isCustomerFailure)
                return Result.Fail<Booking>(customerError);
            
            return await GetRaw(booking => customerData.CustomerId == booking.CustomerId && booking.Id == bookingId);
        }


        public Task<Result<Booking>> GetRaw(string referenceCode)
        {
            return GetRaw(booking => booking.ReferenceCode == referenceCode);
        }
        
        
        public Task<Result<Booking>> GetRaw(int bookingId)
        {
            return GetRaw(booking => booking.Id == bookingId);
        }
        
        
        private async Task<Result<Booking>> GetRaw(Expression<Func<Booking, bool>> filterExpression)
        {
            var bookingData = await _context.Bookings
                .Where(filterExpression)
                .FirstOrDefaultAsync();

            return bookingData.Equals(default)
                ? Result.Fail<Booking>("Could not get booking data")
                : Result.Ok(bookingData);
        }


        private AccommodationBookingInfo ConvertToBookingInfo(Booking booking)
        {
            return new AccommodationBookingInfo(booking.Id,
                JsonConvert.DeserializeObject<AccommodationBookingDetails>(booking.BookingDetails),
                JsonConvert.DeserializeObject<BookingAvailabilityInfo>(booking.ServiceDetails),
                booking.CompanyId);
        }
        
        
        /// <summary>
        /// Get all booking of the current customer
        /// </summary>
        /// <returns>List of the slim booking models </returns>
        public async Task<Result<List<SlimAccommodationBookingInfo>>> GetAll()
        {
            var (_, isFailure, customerData, error) = await _customerContext.GetCustomerInfo();

            if (isFailure)
                return Result.Fail<List<SlimAccommodationBookingInfo>>(error);

            var bookingData = await _context.Bookings
                .Where(b => b.CustomerId == customerData.CustomerId)
                .Select(b =>
                    new SlimAccommodationBookingInfo(b)
                ).ToListAsync();

            return Result.Ok(bookingData);
        }


        public async Task<Result<Booking, ProblemDetails>> SendCancelBookingRequest(int bookingId)
        {
            var (_, isFailure, user, error) = await GetUserInfo();
            if (isFailure)
                return ProblemDetailsBuilder.Fail<Booking>(error);

            var booking = await _context.Bookings
                .SingleOrDefaultAsync(b => b.Id == bookingId && (user.Type == UserTypes.ServiceAccount || b.CustomerId == user.Id));

            if (booking is null)
                return ProblemDetailsBuilder.Fail<Booking>($"Could not find booking with ID '{bookingId}'");

            if (booking.Status == BookingStatusCodes.Cancelled)
                return ProblemDetailsBuilder.Fail<Booking>("Booking was already cancelled");

            var (_, isCancelFailure,_,cancelError) = await ExecuteBookingCancel();
            
            return isCancelFailure 
                ? ProblemDetailsBuilder.Fail<Booking>(error) 
                : Result.Ok<Booking, ProblemDetails>(booking);


            Task<Result<VoidObject, ProblemDetails>> ExecuteBookingCancel()
                => _dataProviderClient.Post(new Uri(_options.Netstorming + "bookings/accommodations/" + booking.ReferenceCode + "/cancel",
                    UriKind.Absolute));

            Task<Result<UserInfo>> GetUserInfo()
                => _serviceAccountContext.GetUserInfo()
                    .OnFailureCompensate(_customerContext.GetUserInfo);
        }


        public async Task<Result<Booking>> ChangeBookingToCancelled(Booking bookingToCancel)
        {
            bookingToCancel.Status = BookingStatusCodes.Cancelled;
            if (bookingToCancel.PaymentStatus == BookingPaymentStatuses.Authorized)
                bookingToCancel.PaymentStatus = BookingPaymentStatuses.Voided;
            if (bookingToCancel.PaymentStatus == BookingPaymentStatuses.Captured)
                bookingToCancel.PaymentStatus = BookingPaymentStatuses.Refunded;

            var currentDetails = JsonConvert.DeserializeObject<AccommodationBookingDetails>(bookingToCancel.BookingDetails);
            bookingToCancel.BookingDetails = JsonConvert.SerializeObject(new AccommodationBookingDetails(currentDetails, BookingStatusCodes.Cancelled));

            _context.Bookings.Update(bookingToCancel);
            await _context.SaveChangesAsync();

            return Result.Ok(bookingToCancel);
        }
        
        // TODO: Replace method when will be added other services 
        private Task<bool> AreExistBookingsForItn(string itn, int customerId)
            => _context.Bookings.Where(b => b.CustomerId == customerId && b.ItineraryNumber == itn).AnyAsync();


        private readonly EdoContext _context;
        private readonly ICustomerContext _customerContext;
        private readonly IServiceAccountContext _serviceAccountContext;
        private readonly IDataProviderClient _dataProviderClient;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly DataProviderOptions _options;
        private readonly ITagProcessor _tagProcessor;
    }
}