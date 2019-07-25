using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Customers;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Customers;

namespace HappyTravel.Edo.Api.Services.Customers
{
    public interface ICustomerService
    {
        Task<Result<Customer>> Create(CustomerRegistrationInfo customerRegistration);
        IList<Currency> GetAvailableCurrencies();
        IList<PaymentMethod> GetAvailablePaymentMethods();
    }
}