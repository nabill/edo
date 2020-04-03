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

        Task<Result<Customer>> GetMasterCustomer(int counterpartyId);

        Task<CustomerEditableInfo> UpdateCurrentCustomer(CustomerEditableInfo newInfo);

        Task<Result<List<SlimCustomerInfo>>> GetCustomers(int counterpartyId, int branchId = default);

        Task<Result<CustomerInfoInBranch>> GetCustomer(int counterpartyId, int branchId, int customerId);
    }
}