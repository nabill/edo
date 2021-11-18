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

            var (isSuccess, _, _) = await service.Disable(1, Agent);

            Assert.True(isSuccess);
            Assert.False(edoContext.AgentAgencyRelations.Single(a => a.AgencyId == 1 && a.AgentId == 1).IsActive);
        }


        [Fact]
        public async Task Enabling_agent_must_change_relation()
        {
            var (service, edoContext) = SetupData();

            var (isSuccess, _, _) = await service.Enable(2, Agent);

            Assert.True(isSuccess);
            Assert.True(edoContext.AgentAgencyRelations.Single(a => a.AgencyId == 1 && a.AgentId == 2).IsActive);
        }


        [Fact]
        public async Task Disabling_nonexistent_agent_must_fail()
        {
            var (service, _) = SetupData();

            var (_, isFailure, _) = await service.Disable(5, Agent);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Enabling_nonexistent_agent_must_fail()
        {
            var (service, _) = SetupData();

            var (_, isFailure, _) = await service.Enable(5, Agent);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Disabling_self_must_fail()
        {
            var (service, _) = SetupData();

            var (_, isFailure, error) = await service.Disable(3, Agent);

            Assert.True(isFailure);
        }


        private (AgentStatusManagementService, EdoContext) SetupData()
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

            var edoContextMock = MockEdoContextFactory.Create();

            var service = new AgentStatusManagementService(edoContextMock.Object);
            edoContextMock.Setup(x => x.AgentAgencyRelations).Returns(DbSetMockProvider.GetDbSetMock(relations));

            return (service, edoContextMock.Object);
        }
        
        private static AgentContext Agent => AgentContextFactory.CreateWithCounterpartyAndAgency(3, 1);
    }
}
