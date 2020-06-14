using System;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentValidation;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Mailing;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings;
using HappyTravel.Money.Helpers;
using HappyTravel.Money.Models;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Mailing
{
    public class BookingMailingService : IBookingMailingService
    {
        public BookingMailingService(MailSenderWithCompanyInfo mailSender,
            IBookingDocumentsService bookingDocumentsService,
            IBookingRecordsManager bookingRecordsManager,
            IOptions<BookingMailingOptions> options)
        {
            _bookingDocumentsService = bookingDocumentsService;
            _bookingRecordsManager = bookingRecordsManager;
            _mailSender = mailSender;
            _options = options.Value;
        }


        public Task<Result> SendVoucher(int bookingId, string email, AgentInfo agent, string languageCode)
        {
            return _bookingDocumentsService.GenerateVoucher(bookingId, agent, languageCode)
                .Bind(voucher =>
                {
                    var voucherData = new VoucherData
                    {
                        Accommodation = voucher.Accommodation,
                        AgentName = voucher.AgentName,
                        BookingId = voucher.BookingId,
                        DeadlineDate = FormatDate(voucher.DeadlineDate),
                        NightCount = voucher.NightCount,
                        ReferenceCode = voucher.ReferenceCode,
                        RoomDetails = voucher.RoomDetails,
                        CheckInDate = FormatDate(voucher.CheckInDate),
                        CheckOutDate = FormatDate(voucher.CheckOutDate),
                        MainPassengerName = voucher.MainPassengerName
                    };

                    return SendEmail(email, _options.VoucherTemplateId, voucherData);
                });
        }


        public Task<Result> SendInvoice(int bookingId, string email, AgentInfo agent, string languageCode)
        {
            return _bookingDocumentsService.GetActualInvoice(bookingId, agent)
                .Bind(invoice =>
                {
                    var (registrationInfo, data) = invoice;
                    var invoiceData = new InvoiceData
                    {
                        Id = registrationInfo.Id,
                        BuyerDetails = data.BuyerDetails,
                        InvoiceDate = FormatDate(registrationInfo.Date),
                        InvoiceItems = data.InvoiceItems
                            .Select(i => new InvoiceData.InvoiceItem
                            {
                                Number = i.Number,
                                Price = FormatPrice(i.Price),
                                Total = FormatPrice(i.Total),
                                AccommodationName = i.AccommodationName,
                                RoomDescription = i.RoomDescription
                            })
                            .ToList(),
                        TotalPrice = FormatPrice(data.TotalPrice),
                        CurrencyCode = data.TotalPrice.Currency.ToString(),
                        ReferenceCode = data.ReferenceCode,
                        SellerDetails = data.SellerDetails,
                        PayDueDate = FormatDate(data.PayDueDate)
                    };

                    return SendEmail(email, _options.InvoiceTemplateId, invoiceData);
                });
        }


        public Task<Result> NotifyBookingCancelled(string referenceCode, string email, string agentName)
            => _mailSender.Send(_options.BookingCancelledTemplateId, email, new BookingCancelledData
            {
                AgentName = agentName,
                ReferenceCode = referenceCode
            });


        public Task<Result> NotifyDeadlineApproaching(int bookingId, string email)
        {
            return _bookingRecordsManager.Get(bookingId)
                .Bind(booking =>
                {
                    var roomDescriptions = booking.Rooms
                        .Select(r => r.ContractDescription);

                    var passengers = booking.Rooms
                        .SelectMany(r => r.Passengers)
                        .Select(p => $"{p.FirstName} {p.LastName}");
                    
                    var deadlineData = new BookingDeadlineData
                    {
                        BookingId = booking.Id,
                        RoomDescriptions = string.Join(", ", roomDescriptions),
                        Passengers = string.Join(", ", passengers),
                        ReferenceCode = booking.ReferenceCode,
                        CheckInDate = FormatDate(booking.CheckInDate),
                        CheckOutDate = FormatDate(booking.CheckOutDate),
                        Deadline = FormatDate(booking.DeadlineDate)
                    };
                    
                    return SendEmail(email, _options.DeadlineNotificationTemplateId, deadlineData);
                });
        }

        private Task<Result> SendEmail(string email, string templateId, DataWithCompanyInfo data)
        {
            return Validate()
                .Bind(Send);


            Result Validate()
                => GenericValidator<string>
                    .Validate(setup => setup.RuleFor(e => e).EmailAddress(), email);


            Task<Result> Send() => _mailSender.Send(templateId, email, data);
        }


        private static string FormatDate(DateTime? date)
        {
            return date.HasValue
                ? date.Value.ToString("dd MMMM yyyy")
                : string.Empty;
        }


        private static string FormatPrice(MoneyAmount moneyAmount) => PaymentAmountFormatter.ToCurrencyString(moneyAmount.Amount, moneyAmount.Currency);


        private readonly IBookingDocumentsService _bookingDocumentsService;
        private readonly IBookingRecordsManager _bookingRecordsManager;
        private readonly MailSenderWithCompanyInfo _mailSender;
        private readonly BookingMailingOptions _options;
    }
}