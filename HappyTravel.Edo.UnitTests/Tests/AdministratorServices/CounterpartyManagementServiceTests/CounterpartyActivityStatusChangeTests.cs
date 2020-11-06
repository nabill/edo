using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Management.Enums;
using HappyTravel.Edo.UnitTests.Utility;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.AdministratorServices.CounterpartyManagementServiceTests
{
    public class CounterpartyActivityStatusChangeTests
    {
        public CounterpartyActivityStatusChangeTests()
        {
            _mockCreationHelper = new AdministratorServicesMockCreationHelper();
        }

        // All tests done for one activity status (change status to not active).

        [Fact]
        public async Task Activity_status_change_of_not_existing_counterparty_should_fail()
        {
            var context = _mockCreationHelper.GetContextMock();
            var counterpartyManagementService = _mockCreationHelper.GetCounterpartyManagementService(context.Object);

            var (_, isFailure, error) = await counterpartyManagementService.ChangeCounterpartyActivityStatus(8, ActivityStatus.NotActive, "Test reason");

            Assert.True(isFailure);
        }

        [Fact]
        public async Task Activity_status_change_of_counterparty_with_empty_reason_should_fail()
        {
            var context = _mockCreationHelper.GetContextMock();
            var counterpartyManagementService = _mockCreationHelper.GetCounterpartyManagementService(context.Object);

            var (_, isFailure, error) = await counterpartyManagementService.ChangeCounterpartyActivityStatus(2, ActivityStatus.NotActive, null);

            Assert.True(isFailure);
        }

        [Fact]
        public async Task  Deactivation_of_not_active_counterparty_should_fail()
        {
            var context = _mockCreationHelper.GetContextMock().Object;
            var counterpartyManagementService = _mockCreationHelper.GetCounterpartyManagementService(context);

            var (_, isFailure, error) = await counterpartyManagementService.ChangeCounterpartyActivityStatus(2, ActivityStatus.NotActive, "Test reason");

            Assert.True(isFailure);
        }


        [Fact]
        public async Task  Activity_status_change_of_counterparty_should_change_status()
        {
            var context = _mockCreationHelper.GetContextMock().Object;
            var counterpartyManagementService = _mockCreationHelper.GetCounterpartyManagementService(context);

            var (_, isFailure, error) = await counterpartyManagementService.ChangeCounterpartyActivityStatus(1, ActivityStatus.NotActive, "Test reason");

            var counterparty = context.Counterparties.First(c => c.Id == 1);
            Assert.False(isFailure);
            Assert.False(counterparty.IsActive);
        }


        [Fact]
        public async Task  Activity_status_change_of_counterparty_should_change_status_of_agencies()
        {
            var context = _mockCreationHelper.GetContextMock().Object;
            var counterpartyManagementService = _mockCreationHelper.GetCounterpartyManagementService(context);

            var (_, isFailure, error) = await counterpartyManagementService.ChangeCounterpartyActivityStatus(1, ActivityStatus.NotActive,"Test reason");
            
            var agencies = context.Agencies.Where(ag => ag.CounterpartyId == 1).ToList();
            Assert.False(isFailure);
            Assert.True(agencies.All(ag => !ag.IsActive));
        }
        
        [Fact]
        public async Task  Activity_status_change_of_counterparty_should_change_accounts_status()
        {
            var context = _mockCreationHelper.GetContextMock().Object;
            var counterpartyManagementService = _mockCreationHelper.GetCounterpartyManagementService(context);

            var (_, isFailure, error) =await counterpartyManagementService.ChangeCounterpartyActivityStatus(1, ActivityStatus.NotActive,"Test reason");
            
            var accounts = context.CounterpartyAccounts.Where(ac => ac.Id == 1).ToList();
            Assert.False(isFailure);
            Assert.True(accounts.All(ac => !ac.IsActive));
        }


        [Fact]
        public async Task  Deactivation_of_not_active_agency_should_fail()
        {
            var context = _mockCreationHelper.GetContextMock().Object;
            var counterpartyManagementService = _mockCreationHelper.GetCounterpartyManagementService(context);

            var (_, isFailure, error) = await counterpartyManagementService.ChangeAgencyActivityStatus(3, ActivityStatus.NotActive, "Test reason");

            Assert.True(isFailure);
        }


        [Fact]
        public async Task  Activity_status_change_of_not_existing_agency_should_fail()
        {
            var context = _mockCreationHelper.GetContextMock();
            var counterpartyManagementService = _mockCreationHelper.GetCounterpartyManagementService(context.Object);

            var (_, isFailure, error) = await counterpartyManagementService.ChangeAgencyActivityStatus(105, ActivityStatus.NotActive, "Test reason");

            Assert.True(isFailure);
        }
        
        [Fact]
        public async Task Activity_status_change_of_agency_with_empty_reason_should_fail()
        {
            var context = _mockCreationHelper.GetContextMock();
            var counterpartyManagementService = _mockCreationHelper.GetCounterpartyManagementService(context.Object);

            var (_, isFailure, error) = await counterpartyManagementService.ChangeAgencyActivityStatus(1, ActivityStatus.NotActive, null);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task  Activity_status_change_of_agency_should_change_agency_status()
        {
            var context = _mockCreationHelper.GetContextMock().Object;
            var counterpartyManagementService = _mockCreationHelper.GetCounterpartyManagementService(context);

            var (_, isFailure, error) = await counterpartyManagementService.ChangeAgencyActivityStatus(1, ActivityStatus.NotActive, "Test reason");

            var agency = context.Agencies.Single(ag => ag.Id == 1);
            Assert.False(isFailure);
            Assert.False(agency.IsActive);
        }


        [Fact]
        public async Task  Activity_status_change_of_agency_should_change_agents_status()
        {
            var context = _mockCreationHelper.GetContextMock().Object;
            var counterpartyManagementService = _mockCreationHelper.GetCounterpartyManagementService(context);

            var (_, isFailure, error) = await counterpartyManagementService.ChangeAgencyActivityStatus(1, ActivityStatus.NotActive, "Test reason");
            
            var agentIds = new List<int> {1, 2};
            var agents = context.Agents.Where(ag => agentIds.Contains(ag.Id)).ToList();
            Assert.False(isFailure);
            Assert.True(agents.All(ag => !ag.IsActive));
        }
        
        [Fact]
        public async Task  Activity_status_change_of_agency_should_change_accounts_status()
        {
            var context = _mockCreationHelper.GetContextMock().Object;
            var counterpartyManagementService = _mockCreationHelper.GetCounterpartyManagementService(context);

            var (_, isFailure, error) = await counterpartyManagementService.ChangeAgencyActivityStatus(1,ActivityStatus.NotActive, "Test reason");
            
            var account = context.AgencyAccounts.Single(p => p.Id == 1);
            Assert.False(isFailure);
            Assert.False(account.IsActive);
        }


        [Fact]
        public async Task  Activity_status_change_of_agency_should_change_child_agencies_status()
        {
            var context = _mockCreationHelper.GetContextMock().Object;
            var counterpartyManagementService = _mockCreationHelper.GetCounterpartyManagementService(context);

            var (_, isFailure, error) = await counterpartyManagementService.ChangeAgencyActivityStatus(1, ActivityStatus.NotActive,"Test reason.");
            
            var childAgency = context.Agencies.Single(ag => ag.Id == 4);
            Assert.False(isFailure);
            Assert.False(childAgency.IsActive);
        }


        [Fact]
        public async Task  Activity_status_change_of_default_agency_should_change_counterparty_status()
        {
            var context = _mockCreationHelper.GetContextMock().Object;
            var counterpartyManagementService = _mockCreationHelper.GetCounterpartyManagementService(context);

            var (_, isFailure, error) = await counterpartyManagementService.ChangeAgencyActivityStatus(1, ActivityStatus.NotActive, "Test reason");
            
            var counterparty = context.Counterparties.Single(c => c.Id == 1);
            Assert.False(isFailure);
            Assert.False(counterparty.IsActive);
        }


        private readonly AdministratorServicesMockCreationHelper _mockCreationHelper;
    }
}