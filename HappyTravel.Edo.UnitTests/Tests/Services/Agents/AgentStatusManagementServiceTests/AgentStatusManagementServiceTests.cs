using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Edo.UnitTests.Utility;
using Moq;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Agents.AgentStatusManagementServiceTests
{
    public class AgentStatusManagementServiceTests
    {
        [Fact]
        public async Task Disabling_agent_must_change_relation()
        {
            var (service, edoContext) = SetupData();

            var (isSuccess, _, error) = await service.Disable(1);

            Assert.True(isSuccess);
            Assert.False(edoContext.AgentAgencyRelations.Single(a => a.AgencyId == 1 && a.AgentId == 1).IsActive);
        }


        [Fact]
        public async Task Enabling_agent_must_change_relation()
        {
            var (service, edoContext) = SetupData();

            var (isSuccess, _, error) = await service.Enable(2);

            Assert.True(isSuccess);
            Assert.True(edoContext.AgentAgencyRelations.Single(a => a.AgencyId == 1 && a.AgentId == 2).IsActive);
        }


        [Fact]
        public async Task Disabling_nonexistent_agent_must_fail()
        {
            var (service, edoContext) = SetupData();

            var (_, isFailure, error) = await service.Disable(5);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Enabling_nonexistent_agent_must_fail()
        {
            var (service, edoContext) = SetupData();

            var (_, isFailure, error) = await service.Enable(5);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Disabling_self_must_fail()
        {
            var (service, edoContext) = SetupData();

            var (_, isFailure, error) = await service.Disable(3);

            Assert.True(isFailure);
        }


        private (AgentStatusManagementService service, EdoContext edoContext) SetupData()
        {
            var relations = new[]
            {
                new AgentAgencyRelation
                {
                    AgencyId = 1,
                    AgentId = 1,
                    IsActive = true
                },
                new AgentAgencyRelation
                {
                    AgencyId = 1,
                    AgentId = 2,
                    IsActive = false
                },
                new AgentAgencyRelation
                {
                    AgencyId = 1,
                    AgentId = 3,
                    IsActive = true
                }
            };

            var agentContext = AgentInfoFactory.CreateByWithCounterpartyAndAgency(3, 1, 1);

            var agentContextServiceMock = new Mock<IAgentContextService>();
            agentContextServiceMock.Setup(m => m.GetAgent()).Returns(() => new ValueTask<AgentContext>(agentContext));

            var edoContextMock = MockEdoContextFactory.Create();

            var service = new AgentStatusManagementService(edoContextMock.Object, agentContextServiceMock.Object);
            edoContextMock.Setup(x => x.AgentAgencyRelations).Returns(DbSetMockProvider.GetDbSetMock(relations));

            return (service, edoContextMock.Object);
        }
    }
}
