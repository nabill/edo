using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentValidation;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Services.Accommodations;
using HappyTravel.MailSender;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Mailing
{
    public class BookingMailingService : IBookingMailingService
    {
        public BookingMailingService(IMailSender mailSender,
            IAccommodationBookingManager accommodationBookingManager,
            IBookingDocumentsService bookingDocumentsService,
            IOptions<BookingMailingOptions> options)
        {
            _mailSender = mailSender;
            _options = options.Value;
            _bookingDocumentsService = bookingDocumentsService;
        }


        public Task<Result> SendVoucher(int bookingId, string email)
        {
            return SendEmail(email,
                _options.VoucherTemplateId, 
                () => _bookingDocumentsService.GenerateVoucher(bookingId));
        }


        public Task<Result> SendInvoice(int bookingId, string email)
        {
            return SendEmail(email,
                _options.InvoiceTemplateId, 
                () => _bookingDocumentsService.GenerateInvoice(bookingId));
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


        private Task<Result> SendEmail<T>(string email, string templateId,
            Func<Task<Result<T>>> getSendDataFunction)
        {
            return Validate()
                .OnSuccess(getSendDataFunction)
                .OnSuccess(Send);

            Result Validate() => GenericValidator<string>.Validate(setup => setup.RuleFor(e => e).EmailAddress(), email);

            async Task<Result> Send(T data) => await _mailSender.Send(templateId, email, data);
        }


        private readonly IMailSender _mailSender;
        private readonly BookingMailingOptions _options;
        private readonly IBookingDocumentsService _bookingDocumentsService;
    }
}