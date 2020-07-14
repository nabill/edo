using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.UnitTests.Utility;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.AdministratorServices.CounterpartyManagementServiceTests
{
    public class CounterpartySuspensionTests
    {
        public CounterpartySuspensionTests()
        {
            _mockCreationHelper = new AdministratorServicesMockCreationHelper();
        }


        [Fact]
        public async Task Suspension_of_not_existing_counterparty_should_fail()
        {
            var context = _mockCreationHelper.GetContextMock();
            var counterpartyManagementService = _mockCreationHelper.GetCounterpartyManagementService(context.Object);

            var (_, isFailure, error) = await counterpartyManagementService.SuspendCounterparty(8);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Suspension_of_not_active_counterparty_should_fail()
        {
            var context = _mockCreationHelper.GetContextMock().Object;
            var counterpartyManagementService = _mockCreationHelper.GetCounterpartyManagementService(context);

            var (_, isFailure, error) = await counterpartyManagementService.SuspendCounterparty(2);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Suspension_of_counterparty_should_suspend_counterparty()
        {
            var context = _mockCreationHelper.GetContextMock().Object;
            var counterpartyManagementService = _mockCreationHelper.GetCounterpartyManagementService(context);

            var (_, isFailure, error) = await counterpartyManagementService.SuspendCounterparty(1);

            var counterparty = context.Counterparties.First(c => c.Id == 1);
            Assert.False(isFailure);
            Assert.False(counterparty.IsActive);
        }


        [Fact]
        public async Task Suspension_of_counterparty_should_suspend_agencies()
        {
            var context = _mockCreationHelper.GetContextMock().Object;
            var counterpartyManagementService = _mockCreationHelper.GetCounterpartyManagementService(context);

            var (_, isFailure, error) = await counterpartyManagementService.SuspendCounterparty(1);
            
            var agencies = context.Agencies.Where(ag => ag.CounterpartyId == 1).ToList();
            Assert.False(isFailure);
            Assert.True(agencies.All(ag => !ag.IsActive));
        }
        
        [Fact]
        public async Task Suspension_of_counterparty_should_suspend_accounts()
        {
            var context = _mockCreationHelper.GetContextMock().Object;
            var counterpartyManagementService = _mockCreationHelper.GetCounterpartyManagementService(context);

            var (_, isFailure, error) = await counterpartyManagementService.SuspendCounterparty(1);
            
            var accounts = context.CounterpartyAccounts.Where(ac => ac.Id == 1).ToList();
            Assert.False(isFailure);
            Assert.True(accounts.All(ac => !ac.IsActive));
        }


        [Fact]
        public async Task Suspension_of_not_active_agency_should_fail()
        {
            var context = _mockCreationHelper.GetContextMock().Object;
            var counterpartyManagementService = _mockCreationHelper.GetCounterpartyManagementService(context);

            var (_, isFailure, error) = await counterpartyManagementService.SuspendAgency(3);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Suspension_of_not_existing_agency_should_fail()
        {
            var context = _mockCreationHelper.GetContextMock();
            var counterpartyManagementService = _mockCreationHelper.GetCounterpartyManagementService(context.Object);

            var (_, isFailure, error) = await counterpartyManagementService.SuspendAgency(14);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Suspension_of_agency_should_suspend_agency()
        {
            var context = _mockCreationHelper.GetContextMock().Object;
            var counterpartyManagementService = _mockCreationHelper.GetCounterpartyManagementService(context);

            var (_, isFailure, error) = await counterpartyManagementService.SuspendAgency(1);

            var agency = context.Agencies.Single(ag => ag.Id == 1);
            Assert.False(isFailure);
            Assert.False(agency.IsActive);
        }


        [Fact]
        public async Task Suspension_of_agency_should_suspend_agents()
        {
            var context = _mockCreationHelper.GetContextMock().Object;
            var counterpartyManagementService = _mockCreationHelper.GetCounterpartyManagementService(context);

            var (_, isFailure, error) = await counterpartyManagementService.SuspendAgency(1);
            
            var agentIds = new List<int> {1, 2};
            var agents = context.Agents.Where(ag => agentIds.Contains(ag.Id)).ToList();
            Assert.False(isFailure);
            Assert.True(agents.All(ag => !ag.IsActive));
        }
        
        [Fact]
        public async Task Suspension_of_agency_should_suspend_accounts()
        {
            var context = _mockCreationHelper.GetContextMock().Object;
            var counterpartyManagementService = _mockCreationHelper.GetCounterpartyManagementService(context);

            var (_, isFailure, error) = await counterpartyManagementService.SuspendAgency(1);
            
            var account = context.PaymentAccounts.Single(p => p.Id == 1);
            Assert.False(isFailure);
            Assert.False(account.IsActive);
        }


        [Fact]
        public async Task Suspension_of_agency_should_suspend_child_agencies()
        {
            var context = _mockCreationHelper.GetContextMock().Object;
            var counterpartyManagementService = _mockCreationHelper.GetCounterpartyManagementService(context);

            var (_, isFailure, error) = await counterpartyManagementService.SuspendAgency(1);
            
            var childAgency = context.Agencies.Single(ag => ag.Id == 4);
            Assert.False(isFailure);
            Assert.False(childAgency.IsActive);
        }


        [Fact]
        public async Task Suspension_of_default_agency_should_suspend_counterparty()
        {
            var context = _mockCreationHelper.GetContextMock().Object;
            var counterpartyManagementService = _mockCreationHelper.GetCounterpartyManagementService(context);

            var (_, isFailure, error) = await counterpartyManagementService.SuspendAgency(1);
            
            var counterparty = context.Counterparties.Single(c => c.Id == 1);
            Assert.False(isFailure);
            Assert.False(counterparty.IsActive);
        }


        private readonly AdministratorServicesMockCreationHelper _mockCreationHelper;
    }
}