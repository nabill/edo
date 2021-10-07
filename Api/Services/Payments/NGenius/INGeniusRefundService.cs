using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Models.Payments.Payfort;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.Api.Services.Payments.NGenius
{
    public interface INGeniusRefundService
    {
        Task<List<int>> GetPaymentsForRefund(DateTime? date);

        Task<Result<BatchOperationResult>> RefundPayments(List<int> paymentIds);

        Task<Result<CreditCardRefundResult>> Refund(int paymentId, MoneyAmount amount, string externalId, string referenceCode);
    }
}