using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Edo.UnitTests.Mocks;
using HappyTravel.Edo.UnitTests.Utility;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Agents.PermissionCheckerTests
{
    public class PermissionCheckerTests
    {
        public PermissionCheckerTests()
        {
            var edoContextMock = MockEdoContextFactory.Create();
            edoContextMock.Setup(x => x.AgentRoles)
                .Returns(DbSetMockProvider.GetDbSetMock(_roles));
            edoContextMock.Setup(x => x.AgentAgencyRelations)
                .Returns(DbSetMockProvider.GetDbSetMock(_relations));
            
            _permissionChecker = new PermissionChecker(edoContextMock.Object, new FakeDoubleFlow());
        }
        
        
        [Fact]
        public async Task Should_succeed_if_agent_has_permission()
        {
            var (isSuccess, _, _) = await _permissionChecker.CheckInAgencyPermission(AffiliatedAgent, InAgencyPermissions.ObservePaymentHistory);
            
            Assert.True(isSuccess);
        }
        
        
        [Fact]
        public async Task Should_fail_if_agent_doesnt_have_permission()
        {
            var (isSuccess, _, _) = await _permissionChecker.CheckInAgencyPermission(AffiliatedAgent, InAgencyPermissions.AccommodationAvailabilitySearch);
            
            Assert.False(isSuccess);
        }


        [Fact]
        public async Task Should_fail_if_agent_isnt_affiliated()
        {
            var (_, isFailure, error) = await _permissionChecker.CheckInAgencyPermission(NotAffiliatedAgent, InAgencyPermissions.AccommodationBooking);
            
            Assert.True(isFailure);
            Assert.Equal("The agent isn't affiliated with the agency", error);
        }

        
        private readonly IEnumerable<AgentAgencyRelation> _relations = new[]
        {
            new AgentAgencyRelation
            {
                AgencyId = 1,
                AgentId = 1,
                Type = default,
                IsActive = true,
                AgentRoleIds = new [] { 1 } 
            }
        };

        private readonly IEnumerable<AgentRole> _roles = new[]
        {
            new AgentRole
            {
                Id = 1,
                Name = "Accounts Manager",
                Permissions = InAgencyPermissions.ObserveBalance | InAgencyPermissions.ObservePaymentHistory | InAgencyPermissions.AgencyToChildTransfer
            }
        };

        private static readonly AgentContext AffiliatedAgent = AgentContextFactory.CreateWithCounterpartyAndAgency(1, 1, 1);
        private static readonly AgentContext NotAffiliatedAgent = AgentContextFactory.CreateWithCounterpartyAndAgency(100, 100, 100);
        private readonly IPermissionChecker _permissionChecker;
    }
}