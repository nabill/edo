using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Payments;
using HappyTravel.Money.Extensions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Services.Payments.NGenius
{
    public class NGeniusRefundService : INGeniusRefundService
    {
        public NGeniusRefundService(EdoContext context, IDateTimeProvider dateTimeProvider, INGeniusPaymentService nGeniusPaymentService)
        {
            _context = context;
            _dateTimeProvider = dateTimeProvider;
            _nGeniusPaymentService = nGeniusPaymentService;
        }


        public Task<List<int>> GetPaymentsForRefund(DateTime? date)
        {
            date ??= _dateTimeProvider.UtcNow();
            
            return (from payment in _context.Payments
                    join refund in _context.NGeniusRefunds on payment.Id equals refund.PaymentId
                    where refund.PlannedDate >= date && payment.Status == PaymentStatuses.Captured
                    select payment.Id)
                .ToListAsync();
        }


        public async Task<Result<BatchOperationResult>> RefundPayments(List<int> paymentIds)
        {
            var paymentsWithRefunds = await (from payment in _context.Payments
                    join refund in _context.NGeniusRefunds on payment.Id equals refund.PaymentId
                    where paymentIds.Contains(payment.Id) && payment.Status == PaymentStatuses.Captured
                    select new Tuple<Payment, NGeniusRefund>(payment, refund))
                .ToListAsync();

            if (paymentsWithRefunds.Count != paymentIds.Count)
                return Result.Failure<BatchOperationResult>("Invalid payment ids. Could not find some of requested payments.");
            
            var builder = new StringBuilder();
            var hasErrors = false;

            foreach (var (payment, refund) in paymentsWithRefunds)
            {
                var data = JsonConvert.DeserializeObject<CreditCardPaymentInfo>(payment.Data);
                var (_, isFailure, _, error) = await _nGeniusPaymentService.Refund(paymentId: data.ExternalId,
                    orderReference: data.InternalReferenceCode,
                    captureId: payment.CaptureId,
                    amount: refund.Amount.ToMoneyAmount(refund.Currency));

                if (isFailure)
                    hasErrors = true;
                
                builder.AppendLine(isFailure 
                    ? $"Unable to refund {payment.Id}. Error: `{error}`"
                    : $"Successfully refund {payment.Id}");
            }
            
            return new BatchOperationResult(builder.ToString(), hasErrors);
        }


        private readonly EdoContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly INGeniusPaymentService _nGeniusPaymentService;
    }
}