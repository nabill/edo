using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments
{
    /// <summary>
    ///     Payment request with account
    /// </summary>
    public readonly struct AccountPaymentRequest
    {
        [JsonConstructor]
        public AccountPaymentRequest(string referenceCode)
        {
            ReferenceCode = referenceCode;
        }


        /// <summary>
        ///     Booking reference code
        /// </summary>
        public string ReferenceCode { get; }
    }
}