using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure.Converters;
using HappyTravel.Edo.Api.Models.Customers;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Customers;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HappyTravel.Edo.Api.Services.Customers
{
    public class CustomerSettingsManager : ICustomerSettingsManager
    {
        public CustomerSettingsManager(EdoContext context, IJsonSerializer serializer)
        {
            _context = context;
            _serializer = serializer;
        }


        public async Task SetAppSettings(CustomerInfo customerInfo, JToken appSettings)
        {
            var customer = await GetCustomer(customerInfo);
            customer.AppSettings = appSettings.ToString(Formatting.None);
            _context.Update(customer);
            await _context.SaveChangesAsync();
        }


        public async Task<JToken> GetAppSettings(CustomerInfo customerInfo)
        {
            var settings = await _context.Customers
                    .Where(c => c.Id == customerInfo.CustomerId)
                    .Select(c => c.AppSettings)
                    .SingleOrDefaultAsync() ?? Infrastructure.Constants.Common.EmptyJsonFieldValue;

            return JToken.Parse(settings);
        }


        public async Task SetUserSettings(CustomerInfo customerInfo, CustomerUserSettings userSettings)
        {
            var customer = await GetCustomer(customerInfo);
            customer.UserSettings = _serializer.SerializeObject(userSettings);
            _context.Update(customer);
            await _context.SaveChangesAsync();
        }


        public async Task<CustomerUserSettings> GetUserSettings(CustomerInfo customerInfo)
        {
            var settings = await _context.Customers
                .Where(c => c.Id == customerInfo.CustomerId)
                .Select(c => c.UserSettings)
                .SingleOrDefaultAsync();

            return settings == default
                ? default
                : _serializer.DeserializeObject<CustomerUserSettings>(settings);
        }


        private Task<Customer> GetCustomer(CustomerInfo customerInfo) => _context.Customers
            .SingleOrDefaultAsync(c => c.Id == customerInfo.CustomerId);


        private readonly EdoContext _context;
        private readonly IJsonSerializer _serializer;
    }
}