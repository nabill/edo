using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Customers;
using HappyTravel.Edo.Api.Models.Users;

namespace HappyTravel.Edo.Api.Services.Customers
{
    public interface ICustomerContext
    {
        [Obsolete("Use GetCustomer instead")]
        ValueTask<Result<CustomerInfo>> GetCustomerInfo();
        
        CustomerInfo GetCustomer();

        Task<Result<UserInfo>> GetUserInfo();

        Task<List<CustomerCompanyInfo>> GetCustomerCompanies();
        
        ValueTask<Result<CustomerInfo>> SetCustomerInfo(int customerId);
    }
}