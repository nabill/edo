using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Customers;
using Newtonsoft.Json.Linq;

namespace HappyTravel.Edo.Api.Services.Customers
{
    public interface ICustomerSettingsManager
    {
        Task SetAppSettings(CustomerInfo customerInfo, JToken appSettings);

        Task<JToken> GetAppSettings(CustomerInfo customerInfo);

        Task SetUserSettings(CustomerInfo customerInfo, CustomerUserSettings userSettings);

        Task<CustomerUserSettings> GetUserSettings(CustomerInfo customerInfo);
    }
}