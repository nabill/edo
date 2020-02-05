using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments
{
    /// <summary>
    ///     Payment request with account
    /// </summary>
    public readonly struct BookingAccountPaymentRequest
    {
        [JsonConstructor]
        public BookingAccountPaymentRequest(string referenceCode)
        {
            ReferenceCode = referenceCode;
        }


        /// <summary>
        ///     Booking reference code
        /// </summary>
        public string ReferenceCode { get; }
    }
}