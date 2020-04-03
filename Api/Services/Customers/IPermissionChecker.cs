using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Customers;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Services.Customers
{
    public interface IPermissionChecker
    {
        ValueTask<Result> CheckInCounterpartyPermission(CustomerInfo customer, InCounterpartyPermissions permission);
    }
}