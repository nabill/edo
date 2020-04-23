using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure.Converters;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Edo.UnitTests.Infrastructure;
using HappyTravel.Edo.UnitTests.Infrastructure.DbSetMocks;
using Moq;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Agents.AgentSettings
{
    public class AppSettings
    {
        public AppSettings(Mock<EdoContext> edoContextMock, IJsonSerializer serializer)
        {
            _settingsManager = new AgentSettingsManager(edoContextMock.Object, serializer);
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

        private readonly AgentSettingsManager _settingsManager;
    }
}