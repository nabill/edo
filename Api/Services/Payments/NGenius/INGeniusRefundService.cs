using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Bookings;

namespace HappyTravel.Edo.Api.Services.Payments.NGenius
{
    public interface INGeniusRefundService
    {
        Task<List<int>> GetPaymentsForRefund(DateTime? date);

        Task<Result<BatchOperationResult>> RefundPayments(List<int> paymentIds);
    }
}