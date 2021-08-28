using HappyTravel.Edo.Common.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments.Payfort
{
    public readonly struct CreditCardCaptureResult
    {
        [JsonConstructor]
        public CreditCardCaptureResult(string externalCode, string message, string merchantReference, string captureId)
        {
            ExternalCode = externalCode;
            Message = message;
            MerchantReference = merchantReference;
            CaptureId = captureId;
        }

        public string ExternalCode { get; }
        public string Message { get; }
        public string MerchantReference { get; }
        public string CaptureId { get; }
    }
}