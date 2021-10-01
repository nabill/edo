using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Payments;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Payments;
using HappyTravel.Money.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public class CreditCardPaymentManagementService : ICreditCardPaymentManagementService
    {
        public CreditCardPaymentManagementService(EdoContext context, IDateTimeProvider dateTimeProvider, IBookingPaymentCallbackService bookingPaymentCallbackService)
        {
            _context = context;
            _dateTimeProvider = dateTimeProvider;
            _bookingPaymentCallbackService = bookingPaymentCallbackService;
        }


        public async Task<Result<Payment>> Create(string paymentId, string paymentOrderReference, string bookingReferenceCode, MoneyAmount price, string ipAddress)
        {
            var now = _dateTimeProvider.UtcNow();

            var info = new CreditCardPaymentInfo(customerIp: ipAddress, 
                externalId: paymentId,
                message: string.Empty, 
                authorizationCode: string.Empty, 
                expirationDate: string.Empty,
                internalReferenceCode: paymentOrderReference);

            var payment = new Payment
            {
                Amount = price.Amount,
                Currency = price.Currency,
                AccountNumber = string.Empty,
                Created = now,
                Modified = now,
                Status = PaymentStatuses.Created,
                Data = JsonConvert.SerializeObject(info),
                PaymentMethod = PaymentTypes.CreditCard,
                PaymentProcessor = PaymentProcessors.NGenius,
                ReferenceCode = bookingReferenceCode
            };
            
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            await _bookingPaymentCallbackService.ProcessPaymentChanges(payment);
            return payment;
        }
        
        
        public async Task<Result<Payment>> Get(string referenceCode)
        {
            var payment = await _context.Payments
                .OrderByDescending(p => p.Created)
                .FirstOrDefaultAsync(p => p.PaymentProcessor == PaymentProcessors.NGenius && p.ReferenceCode == referenceCode);

            return payment ?? Result.Failure<Payment>($"Payment for {referenceCode} not found");
        }


        public async Task<Result> SetStatus(Payment payment, PaymentStatuses status)
        {
            payment.Status = status;
            payment.Modified = _dateTimeProvider.UtcNow();
            _context.Update(payment);
            await _context.SaveChangesAsync();
            await _bookingPaymentCallbackService.ProcessPaymentChanges(payment);
            return Result.Success();
        }


        private readonly EdoContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IBookingPaymentCallbackService _bookingPaymentCallbackService;
    }
}