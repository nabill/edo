using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Markups.Templates;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Edo.UnitTests.Utility;
using Moq;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Agents.AgentAgencyEnablementServiceTests
{
    public class AgentAgencyEnablementServiceTests
    {
        public AgentAgencyEnablementServiceTests(Mock<EdoContext> edoContextMock)
        {
            _edoContextMock = edoContextMock;
            SetRelations();

            _agentAgencyEnablementService = new AgentAgencyEnablementService(edoContextMock.Object);
        }


        [Fact]
        public async Task Disabling_agent_must_change_relation()
        {
            SetRelations();

            var (isSuccess, _, error) = await _agentAgencyEnablementService.Disable(1, 1, AgentContext);

            Assert.True(isSuccess);
            Assert.False(AgentAgencyRelations.Single(a => a.AgencyId == 1 && a.AgentId == 1).IsEnabled);
        }


        [Fact]
        public async Task Enabling_agent_must_change_relation()
        {
            SetRelations();

            var (isSuccess, _, error) = await _agentAgencyEnablementService.Enable(1, 2, AgentContext);

            Assert.True(isSuccess);
            Assert.True(AgentAgencyRelations.Single(a => a.AgencyId == 1 && a.AgentId == 2).IsEnabled);
        }


        [Fact]
        public async Task Disabling_agent_from_other_agency_must_fail()
        {
            SetRelations();

            var (_, isFailure, error) = await _agentAgencyEnablementService.Disable(2, 1, AgentContext);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Disabling_agent_from_other_agency_must_not_change_enablement()
        {
            SetRelations();

            var (_, _, error) = await _agentAgencyEnablementService.Disable(2, 1, AgentContext);

            Assert.True(AgentAgencyRelations.Single(a => a.AgencyId == 2 && a.AgentId == 1).IsEnabled);
        }


        [Fact]
        public async Task Enabling_agent_from_other_agency_must_fail()
        {
            SetRelations();

            var (_, isFailure, error) = await _agentAgencyEnablementService.Enable(2, 2, AgentContext);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Enabling_agent_from_other_agency_must_not_change_enablement()
        {
            SetRelations();

            var (_, _, error) = await _agentAgencyEnablementService.Enable(2, 2, AgentContext);

            Assert.True(AgentAgencyRelations.Single(a => a.AgencyId == 2 && a.AgentId == 1).IsEnabled);
        }


        [Fact]
        public async Task Disabling_nonexistent_agent_must_fail()
        {
            SetRelations();

            var (_, isFailure, error) = await _agentAgencyEnablementService.Disable(5, 5, AgentContext);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Enabling_nonexistent_agent_must_fail()
        {
            SetRelations();

            var (_, isFailure, error) = await _agentAgencyEnablementService.Enable(5, 5, AgentContext);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Disabling_self_must_fail()
        {
            SetRelations();

            var (_, isFailure, error) = await _agentAgencyEnablementService.Disable(1, 3, AgentContext);

            Assert.True(isFailure);
        }


        private void SetRelations()
        {
            var relations = new[]
            {
                new AgentAgencyRelation
                {
                    AgencyId = 1,
                    AgentId = 1,
                    IsEnabled = true
                },
                new AgentAgencyRelation
                {
                    AgencyId = 1,
                    AgentId = 2,
                    IsEnabled = false
                },
                new AgentAgencyRelation
                {
                    AgencyId = 1,
                    AgentId = 3,
                    IsEnabled = true
                },
                new AgentAgencyRelation
                {
                    AgencyId = 2,
                    AgentId = 1,
                    IsEnabled = true
                },
                new AgentAgencyRelation
                {
                    AgencyId = 2,
                    AgentId = 2,
                    IsEnabled = false
                }
            };

            _edoContextMock.Setup(x => x.AgentAgencyRelations).Returns(DbSetMockProvider.GetDbSetMock(relations));
        }


        private IEnumerable<AgentAgencyRelation> AgentAgencyRelations => _edoContextMock.Object.AgentAgencyRelations;


        private readonly Mock<EdoContext> _edoContextMock;

        private static readonly AgentContext AgentContext = AgentInfoFactory.CreateByWithCounterpartyAndAgency(3, 1, 1);
        private readonly AgentAgencyEnablementService _agentAgencyEnablementService;
    }
}
