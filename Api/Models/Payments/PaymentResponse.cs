using HappyTravel.Edo.Common.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments
{
    public readonly struct PaymentResponse
    {
        [JsonConstructor]
        public PaymentResponse(string secure3d, PaymentStatuses status)
        {
            Secure3d = secure3d;
            Status = status;
        }

        public string Secure3d { get; }
        public PaymentStatuses Status { get; }
    }
}
