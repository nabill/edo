using System.Collections.Generic;
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
        public AgentStatusManagementServiceTests(Mock<EdoContext> edoContextMock)
        {
            _edoContextMock = edoContextMock;
        }


        [Fact]
        public async Task Disabling_agent_must_change_relation()
        {
            var (service, agentContext) = SetupData();

            var (isSuccess, _, error) = await service.Disable(1, agentContext);

            Assert.True(isSuccess);
            Assert.False(AgentAgencyRelations.Single(a => a.AgencyId == 1 && a.AgentId == 1).IsActive);
        }


        [Fact]
        public async Task Enabling_agent_must_change_relation()
        {
            var (service, agentContext) = SetupData();

            var (isSuccess, _, error) = await service.Enable(2, agentContext);

            Assert.True(isSuccess);
            Assert.True(AgentAgencyRelations.Single(a => a.AgencyId == 1 && a.AgentId == 2).IsActive);
        }


        [Fact]
        public async Task Disabling_nonexistent_agent_must_fail()
        {
            var (service, agentContext) = SetupData();

            var (_, isFailure, error) = await service.Disable(5, agentContext);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Enabling_nonexistent_agent_must_fail()
        {
            var (service, agentContext) = SetupData();

            var (_, isFailure, error) = await service.Enable(5, agentContext);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Disabling_self_must_fail()
        {
            var (service, agentContext) = SetupData();

            var (_, isFailure, error) = await service.Disable(3, agentContext);

            Assert.True(isFailure);
        }


        private (AgentStatusManagementService service, AgentContext agentContext) SetupData()
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


            var service = new AgentStatusManagementService(_edoContextMock.Object);
            _edoContextMock.Setup(x => x.AgentAgencyRelations).Returns(DbSetMockProvider.GetDbSetMock(relations));

            var agentContext = AgentInfoFactory.CreateByWithCounterpartyAndAgency(3, 1, 1);

            return (service, agentContext);
        }


        private IEnumerable<AgentAgencyRelation> AgentAgencyRelations => _edoContextMock.Object.AgentAgencyRelations;


        private readonly Mock<EdoContext> _edoContextMock;
    }
}
