using HappyTravel.Edo.Common.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments
{
    /// <summary>
    ///     Payment response
    /// </summary>
    public readonly struct PaymentResponse
    {
        [JsonConstructor]
        public PaymentResponse(string referenceCode, string secure3d, PaymentStatuses status, string message)
        {
            ReferenceCode = referenceCode;
            Secure3d = secure3d;
            Status = status;
            Message = message;
        }

        /// <summary>
        ///     Booking reference code 
        /// </summary>
        public string ReferenceCode { get; }

        /// <summary>
        ///     3d secure url
        /// </summary>
        public string Secure3d { get; }

        /// <summary>
        ///     Payment status
        /// </summary>
        public PaymentStatuses Status { get; }

        /// <summary>
        ///     Payment message from payfort
        /// </summary>
        public string Message { get; }
    }
}