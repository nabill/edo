using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Customers;

namespace HappyTravel.Edo.Api.Services.Customers
{
    public interface ICustomerContextInternal
    {
        ValueTask<Result<CustomerInfo>> GetCustomerInfo();
    }
}