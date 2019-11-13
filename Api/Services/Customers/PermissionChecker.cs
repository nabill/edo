using CSharpFunctionalExtensions;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Services.Customers
{
    public class PermissionChecker : IPermissionChecker
    {
        public Result CheckInCompanyPermission(CustomerInfo customer, InCompanyPermissions permission)
        {
            return customer.InCompanyPermissions.HasFlag(permission)
                ? Result.Ok()
                : Result.Fail($"Customer does not have permission '{permission}'");
        }
    }
}