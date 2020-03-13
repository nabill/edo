using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Customers;

namespace HappyTravel.Edo.Api.Services.Customers
{
    public interface ICustomerSettingsManager
    {
        Task SetAppSettings(CustomerInfo customerInfo, string appSettings);

        Task<string> GetAppSettings(CustomerInfo customerInfo);

        Task SetUserSettings(CustomerInfo customerInfo, CustomerUserSettings userSettings);

        Task<CustomerUserSettings> GetUserSettings(CustomerInfo customerInfo);
    }
}