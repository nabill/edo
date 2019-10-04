using HappyTravel.Edo.Common.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments
{
    /// <summary>
    ///     Payment response
    /// </summary>
    public readonly struct PaymentResponse
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="secure3d">3d secure url</param>
        /// <param name="status">Payment status</param>
        [JsonConstructor]
        public PaymentResponse(string secure3d, PaymentStatuses status)
        {
            Secure3d = secure3d;
            Status = status;
        }

        /// <summary>
        ///     3d secure url
        /// </summary>
        public string Secure3d { get; }
        /// <summary>
        ///     Payment status
        /// </summary>
        public PaymentStatuses Status { get; }
    }
}
