using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Data.Customers;

namespace HappyTravel.Edo.Api.Services.Customers
{
    public interface ICustomerContext
    {
        ValueTask<Result<CustomerInfo>> GetCustomerInfo();
    }
}