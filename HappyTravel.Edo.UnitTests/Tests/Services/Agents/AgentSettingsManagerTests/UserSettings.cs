using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure.Converters;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Edo.UnitTests.Utility;
using HappyTravel.Money.Enums;
using Moq;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Agents.AgentSettingsManagerTests
{
    public class UserSettings
    {
        public UserSettings(Mock<EdoContext> edoContextMock, IJsonSerializer serializer)
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
        [MemberData(nameof(SettingsList))]
        public async Task Stored_settings_should_be_retrieved(AgentUserSettings settings)
        {
            var agent = AgentInfoFactory.GetByAgentId(1);
            await _settingsManager.SetUserSettings(agent, settings);
            var storedSettings = await _settingsManager.GetUserSettings(agent);
            
            Assert.Equal(settings, storedSettings);
        }
        
        private readonly AgentSettingsManager _settingsManager;

        public static readonly IEnumerable<object[]> SettingsList = new[]
        {
            new object[] {new AgentUserSettings(true, Currencies.EUR, Currencies.EUR)},
            new object[] {new AgentUserSettings(false, Currencies.USD, Currencies.EUR)},
            new object[] {new AgentUserSettings(false, Currencies.EUR, Currencies.EUR)},
            new object[] {new AgentUserSettings(true, Currencies.USD, Currencies.EUR)},
            new object[] {default(AgentUserSettings)}
        };
    }
}