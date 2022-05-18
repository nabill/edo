using HappyTravel.Edo.Common.Enums;
using Newtonsoft.Json;

namespace Api.Models.Bookings
{
    public readonly struct BookingRecalculatePriceRequest
    {
        [JsonConstructor]
        public BookingRecalculatePriceRequest(PaymentTypes paymentType)
        {
            PaymentType = paymentType;
        }


        public PaymentTypes PaymentType { get; }
    }
}