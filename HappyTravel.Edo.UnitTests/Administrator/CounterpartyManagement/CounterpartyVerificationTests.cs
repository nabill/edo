using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.AdministratorServices;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Administrator.CounterpartyManagement
{
    public class CounterpartyVerificationTests
    {
        public CounterpartyVerificationTests()
        {
            var mockCreationHelper = new MockCreationHelper();
            _context = mockCreationHelper.GetContextMock().Object;
            _counterpartyManagementService = mockCreationHelper.GetCounterpartyManagementService(_context);
        }


        [Fact]
        public async Task Not_existing_counterparty_verification_as_full_accessed_should_fail()
        {
            var (_, isFailure, error) = await _counterpartyManagementService.VerifyAsFullyAccessed(7, "Test reason");
            Assert.True(isFailure);
        }


        [Fact]
        public async Task Not_existing_counterparty_verification_as_read_only_should_fail()
        {
            var (_, isFailure, error) = await _counterpartyManagementService.VerifyAsReadOnly(7, "Test reason");
            Assert.True(isFailure);
        }


        [Fact]
        public async Task Verify_as_full_accessed_should_update_counterparty_state()
        {
            var (_, isFailure, error) = await _counterpartyManagementService.VerifyAsFullyAccessed(1, "Test reason");
            var counterparty = _context.Counterparties.Single(c => c.Id == 1);

            Assert.False(isFailure);
            Assert.True(counterparty.State == CounterpartyStates.FullAccess && counterparty.VerificationReason.Contains("Test reason"));
        }


        [Fact]
        public async Task Verify_as_full_accessed_should_update_inAgencyPermissions()
        {
            var (_, isFailure, error) = await _counterpartyManagementService.VerifyAsFullyAccessed(1, "Test reason");
            var agencies = _context.Agencies.Where(a => a.CounterpartyId == 1).ToList();
            var relations = (from r in _context.AgentAgencyRelations
                join ag in agencies
                    on r.AgencyId equals ag.Id
                select r).ToList();

            Assert.False(isFailure);
            Assert.True(relations.All(r
                => r.InAgencyPermissions.HasFlag(InAgencyPermissions.PermissionManagement) && r.Type == AgentAgencyRelationTypes.Master
                    ? r.InAgencyPermissions == PermissionSets.FullAccessMaster
                    : r.InAgencyPermissions == PermissionSets.FullAccessDefault));
        }


        [Fact]
        public async Task Verify_as_read_only_should_update_counterparty_state()
        {
            var (_, isFailure, error) = await _counterpartyManagementService.VerifyAsReadOnly(1, "Test reason");
            var counterparty = _context.Counterparties.Single(c => c.Id == 1);

            Assert.False(isFailure);
            Assert.True(counterparty.State == CounterpartyStates.ReadOnly && counterparty.VerificationReason.Contains("Test reason"));
        }
        
        
        [Fact]
        public async Task Verify_as_read_only_should_update_inAgencyPermissions()
        {
            var (_, isFailure, error) = await _counterpartyManagementService.VerifyAsReadOnly(1, "Test reason");
            var agencies = _context.Agencies.Where(a => a.CounterpartyId == 1).ToList();
            var relations = (from r in _context.AgentAgencyRelations
                join ag in agencies
                    on r.AgencyId equals ag.Id
                select r).ToList();

            Assert.False(isFailure);
            Assert.True(relations.All(r
                => r.InAgencyPermissions.HasFlag(InAgencyPermissions.PermissionManagement) && r.Type == AgentAgencyRelationTypes.Master
                    ? r.InAgencyPermissions == PermissionSets.ReadOnlyMaster
                    : r.InAgencyPermissions == PermissionSets.ReadOnlyDefault));
        }
        
        [Fact]
        public async Task Verify_as_read_only_should_update_accounts()
        {
            var (_, isFailure, error) = await _counterpartyManagementService.VerifyAsReadOnly(1, "Test reason");
            var agencies = _context.Agencies.Where(a => a.CounterpartyId == 1).ToList();
            
            Assert.False(isFailure);
            Assert.True(_context.CounterpartyAccounts.SingleOrDefaultAsync(c => c.CounterpartyId == 1) != null);
            Assert.True(agencies.All(a => _context.PaymentAccounts.Any(ac => ac.AgencyId == a.Id)));
        }


        private readonly EdoContext _context;
        private readonly ICounterpartyManagementService _counterpartyManagementService;
    }
}