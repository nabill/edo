using System;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Models.Payments.Payfort;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Services.Payments.CreditCards
{
    public static class CreditCardResultsConverter
    {
        public static PaymentStatuses ToPaymentStatus(this CreditCardPaymentStatuses status)
        {
            switch (status)
            {
                case CreditCardPaymentStatuses.Created: return PaymentStatuses.Created;
                case CreditCardPaymentStatuses.Success: return PaymentStatuses.Authorized;
                case CreditCardPaymentStatuses.Secure3d: return PaymentStatuses.Secure3d;
                case CreditCardPaymentStatuses.Failed: return PaymentStatuses.Failed;
                default: throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }
        
        public static PaymentResponse ToPaymentResponse(this CreditCardPaymentResult result) => new PaymentResponse(result.Secure3d, result.Status, result.Message);
    }
}