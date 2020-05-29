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
            IOptions<BookingMailingOptions> options)
        {
            _bookingDocumentsService = bookingDocumentsService;
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
            return _bookingDocumentsService.GenerateInvoice(bookingId, agent, languageCode)
                .Bind(invoice =>
                {
                    var invoiceData = new InvoiceData
                    {
                        Id = invoice.Id,
                        BuyerDetails = invoice.BuyerDetails,
                        InvoiceDate = FormatDate(invoice.InvoiceDate),
                        InvoiceItems = invoice.InvoiceItems
                            .Select(i => new InvoiceData.InvoiceItem
                            {
                                Number = i.Number,
                                Price = FormatPrice(i.Price),
                                Total = FormatPrice(i.Total),
                                AccommodationName = i.AccommodationName,
                                RoomDescription = i.RoomDescription
                            })
                            .ToList(),
                        TotalPrice = FormatPrice(invoice.TotalPrice),
                        CurrencyCode = invoice.TotalPrice.Currency.ToString(),
                        ReferenceCode = invoice.ReferenceCode,
                        SellerDetails = invoice.SellerDetails,
                        PayDueDate = FormatDate(invoice.PayDueDate)
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
        private readonly MailSenderWithCompanyInfo _mailSender;
        private readonly BookingMailingOptions _options;
    }
}