using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure.Converters;
using HappyTravel.Edo.Api.Services.Customers;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Customers;
using HappyTravel.Edo.UnitTests.Infrastructure;
using HappyTravel.Edo.UnitTests.Infrastructure.DbSetMocks;
using Moq;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Customers.CustomerSettings
{
    public class AppSettings
    {
        public AppSettings(Mock<EdoContext> edoContextMock, IJsonSerializer serializer)
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
        [InlineData(null)]
        [InlineData("")]
        [InlineData("{}")]
        [InlineData("{ \"key\": \"value\", \"number\": 1}")]
        public async Task Stored_settings_should_be_retrieved(string settings)
        {
            var customer = CustomerInfoFactory.GetByCustomerId(1);
            await _settingsManager.SetAppSettings(customer, settings);
            var storedSettings = await _settingsManager.GetAppSettings(customer);
            
            Assert.Equal(settings, storedSettings);
        }

        private readonly CustomerSettingsManager _settingsManager;
    }
}