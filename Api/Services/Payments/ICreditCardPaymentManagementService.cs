using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Payments;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public interface ICreditCardPaymentManagementService
    {
        Task<Result<Payment>> Create(string paymentId, string paymentOrderReference, string bookingReferenceCode, MoneyAmount price, 
            string ipAddress);
        
        Task<Result<Payment>> Get(string referenceCode);

        Task<Result> SetStatus(Payment payment, PaymentStatuses status);
    }
}