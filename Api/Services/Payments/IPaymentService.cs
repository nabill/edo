using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Customers;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public interface IPaymentService
    {
        IReadOnlyCollection<Currencies> GetCurrencies();
        IReadOnlyCollection<PaymentMethods> GetAvailableCustomerPaymentMethods();
        Task<Result> ReplenishAccount(int accountId, PaymentData payment);
        Task<Result<PaymentResponse>> Pay(PaymentRequest request, string languageCode, string ipAddress, Customer customer, Company company);
    }
}
