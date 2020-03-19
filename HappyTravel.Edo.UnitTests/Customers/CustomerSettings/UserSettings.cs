using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure.Converters;
using HappyTravel.Edo.Api.Models.Customers;
using HappyTravel.Edo.Api.Services.Customers;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Customers;
using HappyTravel.Edo.UnitTests.Infrastructure;
using HappyTravel.Edo.UnitTests.Infrastructure.DbSetMocks;
using HappyTravel.EdoContracts.General.Enums;
using Moq;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Customers.CustomerSettings
{
    public class UserSettings
    {
        public UserSettings(Mock<EdoContext> edoContextMock, IJsonSerializer serializer)
        {
            _settingsManager = new CustomerSettingsManager(edoContextMock.Object, serializer);
            edoContextMock
                .Setup(c => c.Customers)
                .Returns(DbSetMockProvider.GetDbSetMock(new List<Customer>
                {
                    new Customer
                    {
                        Id = 1,
                    }
                }));
        }
        
        [Theory]
        [MemberData(nameof(SettingsList))]
        public async Task Stored_settings_should_be_retrieved(CustomerUserSettings settings)
        {
            var customer = CustomerInfoFactory.GetByCustomerId(1);
            await _settingsManager.SetUserSettings(customer, settings);
            var storedSettings = await _settingsManager.GetUserSettings(customer);
            
            Assert.Equal(settings, storedSettings);
        }
        
        private readonly CustomerSettingsManager _settingsManager;

        public static readonly IEnumerable<object[]> SettingsList = new[]
        {
            new object[] {new CustomerUserSettings(true, Currencies.EUR, Currencies.EUR)},
            new object[] {new CustomerUserSettings(false, Currencies.USD, Currencies.EUR)},
            new object[] {new CustomerUserSettings(false, Currencies.EUR, Currencies.EUR)},
            new object[] {new CustomerUserSettings(true, Currencies.USD, Currencies.EUR)},
            new object[] {default(CustomerUserSettings)}
        };
    }
}