using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Customers;
using HappyTravel.Edo.Data.Customers;

namespace HappyTravel.Edo.Api.Services.Customers
{
    public interface ICustomerService
    {
        Task<Result<Customer>> Add(CustomerEditableInfo customerRegistration, string externalIdentity, string email);

        Task<Result<Customer>> GetMasterCustomer(int companyId);

        Task<CustomerEditableInfo> UpdateCurrentCustomer(CustomerEditableInfo newInfo);

        Task<Result<List<SlimCustomerInfo>>> GetCustomers(int companyId, int branchId = default);

        Task<Result<CustomerInfo>> GetCustomer(int companyId, int branchId, int customerId);
    }
}