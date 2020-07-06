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
            _mockCreationHelper = new MockCreationHelper();
        }


        [Fact]
        public async Task Verification_of_not_existing_counterparty_as_full_accessed_should_fail()
        {
            var context = _mockCreationHelper.GetContextMock().Object;
            var counterpartyManagementService = _mockCreationHelper.GetCounterpartyManagementService(context);

            var (_, isFailure, error) = await counterpartyManagementService.VerifyAsFullyAccessed(7, "Test reason");

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Verification_of_not_existing_counterparty_as_read_only_should_fail()
        {
            var context = _mockCreationHelper.GetContextMock().Object;
            var counterpartyManagementService = _mockCreationHelper.GetCounterpartyManagementService(context);

            var (_, isFailure, error) = await counterpartyManagementService.VerifyAsReadOnly(7, "Test reason");

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Verification_as_full_accessed_should_update_counterparty_state()
        {
            var context = _mockCreationHelper.GetContextMock().Object;
            var counterpartyManagementService = _mockCreationHelper.GetCounterpartyManagementService(context);

            var (_, isFailure, error) = await counterpartyManagementService.VerifyAsFullyAccessed(1, "Test reason");
            var counterparty = context.Counterparties.Single(c => c.Id == 1);

            Assert.False(isFailure);

            Assert.True(counterparty.State == CounterpartyStates.FullAccess && counterparty.VerificationReason.Contains("Test reason"));
        }


        [Fact]
        public async Task Verification_as_full_accessed_should_update_inAgencyPermissions()
        {
            var context = _mockCreationHelper.GetContextMock().Object;
            var counterpartyManagementService = _mockCreationHelper.GetCounterpartyManagementService(context);

            var (_, isFailure, error) = await counterpartyManagementService.VerifyAsFullyAccessed(1, "Test reason");
            var agencies = context.Agencies.Where(a => a.CounterpartyId == 1).ToList();
            var relations = (from r in context.AgentAgencyRelations
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
        public async Task Verification_as_read_only_should_update_counterparty_state()
        {
            var context = _mockCreationHelper.GetContextMock().Object;
            var counterpartyManagementService = _mockCreationHelper.GetCounterpartyManagementService(context);

            var (_, isFailure, error) = await counterpartyManagementService.VerifyAsReadOnly(1, "Test reason");
            var counterparty = context.Counterparties.Single(c => c.Id == 1);

            Assert.False(isFailure);

            Assert.True(counterparty.State == CounterpartyStates.ReadOnly && counterparty.VerificationReason.Contains("Test reason"));
        }


        [Fact]
        public async Task Verification_as_read_only_should_update_inAgencyPermissions()
        {
            var context = _mockCreationHelper.GetContextMock().Object;
            var counterpartyManagementService = _mockCreationHelper.GetCounterpartyManagementService(context);

            var (_, isFailure, error) = await counterpartyManagementService.VerifyAsReadOnly(1, "Test reason");
            var agencies = context.Agencies.Where(a => a.CounterpartyId == 1).ToList();
            var relations = (from r in context.AgentAgencyRelations
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
        public async Task Verification_as_read_only_should_update_accounts()
        {
            var context = _mockCreationHelper.GetContextMock().Object;
            var counterpartyManagementService = _mockCreationHelper.GetCounterpartyManagementService(context);

            var (_, isFailure, error) = await counterpartyManagementService.VerifyAsReadOnly(1, "Test reason");
            var agencies = context.Agencies.Where(a => a.CounterpartyId == 1).ToList();

            Assert.False(isFailure);

            Assert.True(context.CounterpartyAccounts.SingleOrDefaultAsync(c => c.CounterpartyId == 1) != null);

            Assert.True(agencies.All(a => context.PaymentAccounts.Any(ac => ac.AgencyId == a.Id)));
        }


        private readonly MockCreationHelper _mockCreationHelper;
    }
}