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
        

        public DateTime FromDate { get; }
        public DateTime ToDate { get; }
    }
}
