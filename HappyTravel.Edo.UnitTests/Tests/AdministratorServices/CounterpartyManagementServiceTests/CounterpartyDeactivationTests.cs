using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.UnitTests.Utility;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.AdministratorServices.CounterpartyManagementServiceTests
{
    public class CounterpartyDeactivationTests
    {
        public CounterpartyDeactivationTests()
        {
            _mockCreationHelper = new AdministratorServicesMockCreationHelper();
        }


        [Fact]
        public async Task Deactivation_of_not_existing_counterparty_should_fail()
        {
            var context = _mockCreationHelper.GetContextMock();
            var counterpartyManagementService = _mockCreationHelper.GetCounterpartyManagementService(context.Object);

            var (_, isFailure, error) = await counterpartyManagementService.DeactivateCounterparty(8);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Deactivation_of_not_active_counterparty_should_fail()
        {
            var context = _mockCreationHelper.GetContextMock().Object;
            var counterpartyManagementService = _mockCreationHelper.GetCounterpartyManagementService(context);

            var (_, isFailure, error) = await counterpartyManagementService.DeactivateCounterparty(2);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Deactivation_of_counterparty_should_deactivate_counterparty()
        {
            var context = _mockCreationHelper.GetContextMock().Object;
            var counterpartyManagementService = _mockCreationHelper.GetCounterpartyManagementService(context);

            var (_, isFailure, error) = await counterpartyManagementService.DeactivateCounterparty(1);

            var counterparty = context.Counterparties.First(c => c.Id == 1);
            Assert.False(isFailure);
            Assert.False(counterparty.IsActive);
        }


        [Fact]
        public async Task Deactivation_of_counterparty_should_deactivate_agencies()
        {
            var context = _mockCreationHelper.GetContextMock().Object;
            var counterpartyManagementService = _mockCreationHelper.GetCounterpartyManagementService(context);

            var (_, isFailure, error) = await counterpartyManagementService.DeactivateCounterparty(1);
            
            var agencies = context.Agencies.Where(ag => ag.CounterpartyId == 1).ToList();
            Assert.False(isFailure);
            Assert.True(agencies.All(ag => !ag.IsActive));
        }
        
        [Fact]
        public async Task Deactivation_of_counterparty_should_deactivate_accounts()
        {
            var context = _mockCreationHelper.GetContextMock().Object;
            var counterpartyManagementService = _mockCreationHelper.GetCounterpartyManagementService(context);

            var (_, isFailure, error) = await counterpartyManagementService.DeactivateCounterparty(1);
            
            var accounts = context.CounterpartyAccounts.Where(ac => ac.Id == 1).ToList();
            Assert.False(isFailure);
            Assert.True(accounts.All(ac => !ac.IsActive));
        }


        [Fact]
        public async Task Deactivation_of_not_active_agency_should_fail()
        {
            var context = _mockCreationHelper.GetContextMock().Object;
            var counterpartyManagementService = _mockCreationHelper.GetCounterpartyManagementService(context);

            var (_, isFailure, error) = await counterpartyManagementService.DeactivateAgency(3);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Deactivation_of_not_existing_agency_should_fail()
        {
            var context = _mockCreationHelper.GetContextMock();
            var counterpartyManagementService = _mockCreationHelper.GetCounterpartyManagementService(context.Object);

            var (_, isFailure, error) = await counterpartyManagementService.DeactivateAgency(105);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Deactivation_of_agency_should_deactivate_agency()
        {
            var context = _mockCreationHelper.GetContextMock().Object;
            var counterpartyManagementService = _mockCreationHelper.GetCounterpartyManagementService(context);

            var (_, isFailure, error) = await counterpartyManagementService.DeactivateAgency(1);

            var agency = context.Agencies.Single(ag => ag.Id == 1);
            Assert.False(isFailure);
            Assert.False(agency.IsActive);
        }


        [Fact]
        public async Task Deactivation_of_agency_should_deactivate_agents()
        {
            var context = _mockCreationHelper.GetContextMock().Object;
            var counterpartyManagementService = _mockCreationHelper.GetCounterpartyManagementService(context);

            var (_, isFailure, error) = await counterpartyManagementService.DeactivateAgency(1);
            
            var agentIds = new List<int> {1, 2};
            var agents = context.Agents.Where(ag => agentIds.Contains(ag.Id)).ToList();
            Assert.False(isFailure);
            Assert.True(agents.All(ag => !ag.IsActive));
        }
        
        [Fact]
        public async Task Deactivation_of_agency_should_deactivate_accounts()
        {
            var context = _mockCreationHelper.GetContextMock().Object;
            var counterpartyManagementService = _mockCreationHelper.GetCounterpartyManagementService(context);

            var (_, isFailure, error) = await counterpartyManagementService.DeactivateAgency(1);
            
            var account = context.AgencyAccounts.Single(p => p.Id == 1);
            Assert.False(isFailure);
            Assert.False(account.IsActive);
        }


        [Fact]
        public async Task Deactivation_of_agency_should_deactivate_child_agencies()
        {
            var context = _mockCreationHelper.GetContextMock().Object;
            var counterpartyManagementService = _mockCreationHelper.GetCounterpartyManagementService(context);

            var (_, isFailure, error) = await counterpartyManagementService.DeactivateAgency(1);
            
            var childAgency = context.Agencies.Single(ag => ag.Id == 4);
            Assert.False(isFailure);
            Assert.False(childAgency.IsActive);
        }


        [Fact]
        public async Task Deactivation_of_default_agency_should_deactivate_counterparty()
        {
            var context = _mockCreationHelper.GetContextMock().Object;
            var counterpartyManagementService = _mockCreationHelper.GetCounterpartyManagementService(context);

            var (_, isFailure, error) = await counterpartyManagementService.DeactivateAgency(1);
            
            var counterparty = context.Counterparties.Single(c => c.Id == 1);
            Assert.False(isFailure);
            Assert.False(counterparty.IsActive);
        }


        private readonly AdministratorServicesMockCreationHelper _mockCreationHelper;
    }
}