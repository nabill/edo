using CSharpFunctionalExtensions;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Services.Customers
{
    public interface IPermissionChecker
    {
        Result CheckInCompanyPermission(CustomerInfo customer, InCompanyPermissions permission);
    }
}