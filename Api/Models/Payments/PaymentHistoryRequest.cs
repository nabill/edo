using System;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments
{
    public readonly struct PaymentHistoryRequest
    {
        [JsonConstructor]
        public PaymentHistoryRequest(DateTime fromDate, DateTime toDate)
        {
            FromDate = fromDate;
            ToDate = toDate;
        }

        /// <summary>
        /// Get the payment history from this date
        /// </summary>
        public DateTime FromDate { get; }

        /// <summary>
        /// Get the payment history to this date
        /// </summary>
        public DateTime ToDate { get; }
    }
}
