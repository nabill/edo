using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
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
            edoContextMock.Setup(x => x.AgentCounterpartyRelations).Returns(DbSetMockProvider.GetDbSetMock(_relations));

            _agentContextMock = new Mock<IAgentContext>();

            _agentPermissionManagementService = new AgentPermissionManagementService(edoContextMock.Object,
                _agentContextMock.Object, null);
        }


        [Fact]
        public async Task Set_without_permissions_must_fail()
        {
            SetActingAgent(_agentInfoNoPermissions);

            var (_, isFailure, _, error) = await _agentPermissionManagementService
                .SetInCounterpartyPermissions(1, 1, 1, InCounterpartyPermissions.None);

            Assert.True(isFailure);
            Assert.Equal("You have no acceptance to manage agents permissions", error);
        }

        [Fact]
        public async Task Set_with_different_counterparty_must_fail()
        {
            SetActingAgent(_agentInfoDifferentCounterparty);

            var (_, isFailure, _, error) = await _agentPermissionManagementService
                .SetInCounterpartyPermissions(1, 1, 1, InCounterpartyPermissions.None);

            Assert.True(isFailure);
            Assert.Equal("The agent isn't affiliated with the counterparty", error);
        }

        [Fact]
        public async Task Set_relation_not_found_must_fail()
        {
            SetActingAgent(_agentInfoRegular);

            var (_, isFailure, _, error) = await _agentPermissionManagementService
                .SetInCounterpartyPermissions(1, 1, 0, InCounterpartyPermissions.None);

            Assert.True(isFailure);
            Assert.Equal("Could not find relation between the agent 0 and the counterparty 1", error);
        }

        [Fact]
        public async Task Set_revoke_last_management_must_fail()
        {
            SetActingAgent(_agentInfoRegular);

            var (_, isFailure, _, error) = await _agentPermissionManagementService
                .SetInCounterpartyPermissions(1, 1, 2, InCounterpartyPermissions.None);

            Assert.True(isFailure);
            Assert.Equal("Cannot revoke last permission management rights", error);
        }

        [Fact]
        public async Task Set_must_susseed()
        {
            SetActingAgent(_agentInfoRegular);

            var (isSuccess, _, _, _) = await _agentPermissionManagementService
                .SetInCounterpartyPermissions(1, 1, 1, InCounterpartyPermissions.None);

            Assert.True(isSuccess);
        }

        private void SetActingAgent(AgentInfo agent) =>
            _agentContextMock.Setup(x => x.GetAgent()).Returns(new ValueTask<AgentInfo>(agent));

        private readonly IEnumerable<AgentCounterpartyRelation> _relations = new[]
        {
            new AgentCounterpartyRelation
            {
                CounterpartyId = 1,
                AgencyId = 1,
                AgentId = 1,
                Type = AgentCounterpartyRelationTypes.Master,
                InCounterpartyPermissions = InCounterpartyPermissions.PermissionManagementInAgency
            },
            new AgentCounterpartyRelation
            {
                CounterpartyId = 1,
                AgencyId = 1,
                AgentId = 2,
                Type = AgentCounterpartyRelationTypes.Regular,
                InCounterpartyPermissions = InCounterpartyPermissions.PermissionManagementInCounterparty
            }
        };

        private static readonly AgentInfo _agentInfoRegular = AgentInfoFactory.CreateByWithCounterpartyAndAgency(10, 1, 1);
        private static readonly AgentInfo _agentInfoDifferentCounterparty = AgentInfoFactory.CreateByWithCounterpartyAndAgency(2, 2, 1);
        private static readonly AgentInfo _agentInfoNoPermissions = new AgentInfo(
            11, "", "", "", "", "", 1, "", 1, false, InCounterpartyPermissions.None);

        private readonly AgentPermissionManagementService _agentPermissionManagementService;
        private readonly Mock<IAgentContext> _agentContextMock;
    }
}
