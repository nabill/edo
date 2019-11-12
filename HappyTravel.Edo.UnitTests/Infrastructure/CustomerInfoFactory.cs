using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Services.Customers;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.UnitTests.Infrastructure
{
    public static class CustomerInfoFactory
    {
        public static CustomerInfo GetByCustomerId(int customerId)
        {
            return new CustomerInfo(customerId, string.Empty, string.Empty, string.Empty, string.Empty,  string.Empty, 0, string.Empty, Maybe<int>.None, true, InCompanyPermissions.All);
        }


        public static CustomerInfo GetByWithCompanyAndBranch(int customerId, int companyId, int branchId)
        {
            return new CustomerInfo(customerId, string.Empty, string.Empty, string.Empty, string.Empty,  string.Empty, companyId, string.Empty, branchId, true, InCompanyPermissions.All);
        }
    }
}