using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Models.Payments.NGenius
{
    public readonly struct StatusResponse
    {
        public StatusResponse(PaymentStatuses status)
        {
            Status = status;
            Message = $"Payment completed with status: {status}";
        }

        
        public PaymentStatuses Status { get; }
        public string Message { get; }
    }
}