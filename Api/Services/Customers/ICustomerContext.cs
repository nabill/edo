using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace HappyTravel.Edo.Api.Services.Customers
{
    public interface ICustomerContext
    {
        ValueTask<Result<CustomerInfo>> GetCustomerInfo();
        Task<Result> SetAppSettings(string appSettings);
        Task<Result<string>> GetAppSettings();
    }
}