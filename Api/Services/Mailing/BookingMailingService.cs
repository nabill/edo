using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentValidation;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Emails;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Services.Accommodations;
using HappyTravel.Edo.Data.Booking;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Services.Mailing
{
    public class BookingMailingService : IBookingMailingService
    {
        public BookingMailingService(IMailSender mailSender,
            IAccommodationBookingManager accommodationBookingManager,
            IOptions<BookingMailingOptions> options)
        {
            _mailSender = mailSender;
            _options = options.Value;
            _accommodationBookingManager = accommodationBookingManager;
        }


        public async Task<Result> SendVoucher(int bookingId, string email)
        {
            return await SendEmail(bookingId, email, _options.VoucherTemplateId, CreateVoucherData);


            Result<BookingVoucherMailData> CreateVoucherData(
                (AccommodationBookingInfo bookingInfo, BookingAvailabilityInfo serviceDetails, AccommodationBookingDetails bookingDetails) bookingData)
            {
                var serviceDetails = bookingData.serviceDetails;
                var bookingDetails = bookingData.bookingDetails;
                var bookingInfo = bookingData.bookingInfo;

                return Result.Ok(new BookingVoucherMailData
                {
                    BookingId = bookingInfo.BookingId.ToString(),
                    CheckInDate = bookingDetails.CheckInDate.ToString("d"),
                    CheckOutDate = bookingDetails.CheckOutDate.ToString("d"),
                    ReferenceCode = bookingDetails.ReferenceCode,
                    RoomDetails = bookingDetails.RoomDetails.Select(i => i.RoomDetails).ToList(),

                    AccomodationName = serviceDetails.AccommodationName,
                    LocationName = serviceDetails.CityCode,
                    CountryName = serviceDetails.CountryName,
                    BoardBasis = serviceDetails.Agreement.BoardBasis,
                    BoardBasisCode = serviceDetails.Agreement.BoardBasisCode,
                    ContractType = serviceDetails.Agreement.ContractType,
                    MealPlan = serviceDetails.Agreement.MealPlan,
                    MealPlanCode = serviceDetails.Agreement.MealPlanCode,
                    TariffCode = serviceDetails.Agreement.TariffCode,
                });
            }
        }


        public async Task<Result> SendInvoice(int bookingId, string email)
        {
            return await SendEmail(bookingId, email, _options.InvoiceTemplateId, CreateInvoiceData);


            Result<BookingInvoiceMailData> CreateInvoiceData(
                (AccommodationBookingInfo bookingInfo, BookingAvailabilityInfo serviceDetails, AccommodationBookingDetails bookingDetails) bookingData)
            {
                var serviceDetails = bookingData.serviceDetails;
                var bookingDetails = bookingData.bookingDetails;

                return Result.Ok(new BookingInvoiceMailData
                {
                    CheckInDate = bookingDetails.CheckInDate.ToString("d"),
                    CheckOutDate = bookingDetails.CheckOutDate.ToString("d"),
                    RoomDetails = bookingDetails.RoomDetails.Select(i => i.RoomDetails).ToList(),
                    CurrencyCode = serviceDetails.Agreement.Price.CurrencyCode,
                    PriceTotal = serviceDetails.Agreement.Price.NetTotal.ToString(CultureInfo.InvariantCulture),
                    PriceGross = serviceDetails.Agreement.Price.Gross.ToString(CultureInfo.InvariantCulture),
                    TariffCode = serviceDetails.Agreement.TariffCode,
                    AccomodationName = serviceDetails.AccommodationName,
                    LocationName = serviceDetails.CityCode,
                    CountryName = serviceDetails.CountryName
                });
            }
        }


        public Task<Result> NotifyBookingCancelled(BookingCancelledMailData data)
        {
            var templateId = _options.BookingCancelledTemplateId;

            var payload = new
            {
                referenceCode = data.ReferenceCode,
                customerName = data.CustomerName
            };

            return _mailSender.Send(templateId, data.Email, payload);
        }


        private async Task<Result> SendEmail<T>(int bookingId, string email, string templateId,
            Func<(AccommodationBookingInfo bookingInfo, BookingAvailabilityInfo serviceDetails, AccommodationBookingDetails bookingDetails), Result<T>>
                getSendData)
        {
            return await Validate()
                .OnSuccess(GetBookingData)
                .OnSuccess(getSendData)
                .OnSuccess(Send);

            Result Validate() => GenericValidator<string>.Validate(setup => setup.RuleFor(e => e).EmailAddress(), email);


            async Task<Result<(AccommodationBookingInfo, BookingAvailabilityInfo, AccommodationBookingDetails)>> GetBookingData()
            {
                var (_, isFailure, bookingInfo, error) = await _accommodationBookingManager.Get(bookingId);

                if (isFailure)
                    return Result.Fail<(AccommodationBookingInfo, BookingAvailabilityInfo, AccommodationBookingDetails)>(error);

                return Result.Ok((bookingInfo, bookingInfo.ServiceDetails, bookingInfo.BookingDetails));
            }


            async Task<Result> Send(T data) => await _mailSender.Send(templateId, email, data);
        }


        private readonly IMailSender _mailSender;
        private readonly BookingMailingOptions _options;
        private readonly IAccommodationBookingManager _accommodationBookingManager;
    }
}