using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Edo.UnitTests.Infrastructure;
using HappyTravel.Edo.UnitTests.Infrastructure.DbSetMocks;
using Moq;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Agents.Service
{
    public class AgentPermissionManagementServiceTests
    {
        public AgentPermissionManagementServiceTests(Mock<EdoContext> edoContextMock)
        {
            edoContextMock.Setup(x => x.AgentAgencyRelations).Returns(DbSetMockProvider.GetDbSetMock(_relations));

            _agentContextMock = new Mock<IAgentContext>();

            _agentPermissionManagementService = new AgentPermissionManagementService(edoContextMock.Object,
                _agentContextMock.Object);
        }

        [Fact]
        public async Task Set_relation_not_found_must_fail()
        {
            SetActingAgent(_agentInfoRegular);

            var (_, isFailure, _, error) = await _agentPermissionManagementService
                .SetInAgencyPermissions(1, 0, InAgencyPermissions.None);

            Assert.True(isFailure);
            Assert.Equal("Could not find relation between the agent 0 and the agency 1", error);
        }

        [Fact]
        public async Task Set_revoke_last_management_must_fail()
        {
            SetActingAgent(_agentInfoRegular);

            var (_, isFailure, _, error) = await _agentPermissionManagementService
                .SetInAgencyPermissions(1, 2, InAgencyPermissions.None);

            Assert.True(isFailure);
            Assert.Equal("Cannot revoke last permission management rights", error);
        }

        [Fact]
        public async Task Set_must_susseed()
        {
            SetActingAgent(_agentInfoRegular);

            var (isSuccess, _, _, _) = await _agentPermissionManagementService
                .SetInAgencyPermissions(1, 1, InAgencyPermissions.None);

            Assert.True(isSuccess);
        }

        private void SetActingAgent(AgentInfo agent) =>
            _agentContextMock.Setup(x => x.GetAgent()).Returns(new ValueTask<AgentInfo>(agent));

        private readonly IEnumerable<AgentAgencyRelation> _relations = new[]
        {
            new AgentAgencyRelation
            {
                AgencyId = 1,
                AgentId = 1,
                Type = AgentAgencyRelationTypes.Master,
                InAgencyPermissions = InAgencyPermissions.AgentInvitation
            },
            new AgentAgencyRelation
            {
                AgencyId = 1,
                AgentId = 2,
                Type = AgentAgencyRelationTypes.Regular,
                InAgencyPermissions = InAgencyPermissions.PermissionManagement
            }
        };

        private static readonly AgentInfo _agentInfoRegular = AgentInfoFactory.CreateByWithCounterpartyAndAgency(10, 1, 1);

        private readonly AgentPermissionManagementService _agentPermissionManagementService;
        private readonly Mock<IAgentContext> _agentContextMock;
    }
}
