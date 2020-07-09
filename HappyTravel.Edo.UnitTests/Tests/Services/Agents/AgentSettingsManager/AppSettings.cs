using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure.Converters;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Edo.UnitTests.Utility;
using Moq;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Agents.AgentSettingsManager
{
    public class AppSettings
    {
        public AppSettings(Mock<EdoContext> edoContextMock, IJsonSerializer serializer)
        {
            _settingsManager = new Api.Services.Agents.AgentSettingsManager(edoContextMock.Object, serializer);
            edoContextMock
                .Setup(c => c.Agents)
                .Returns(DbSetMockProvider.GetDbSetMock(new List<Agent>
                {
                    new Agent
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
            var agent = AgentInfoFactory.GetByAgentId(1);
            await _settingsManager.SetAppSettings(agent, settings);
            var storedSettings = await _settingsManager.GetAppSettings(agent);
            
            Assert.Equal(settings, storedSettings);
        }

        private readonly Api.Services.Agents.AgentSettingsManager _settingsManager;
    }
}