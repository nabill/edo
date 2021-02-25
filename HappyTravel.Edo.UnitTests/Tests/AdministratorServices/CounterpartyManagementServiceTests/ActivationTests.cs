using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.UnitTests.Utility;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.AdministratorServices.CounterpartyManagementServiceTests
{
    public class ActivationTests : IDisposable
    {
        public ActivationTests()
        {
            _mockCreationHelper = new AdministratorServicesMockCreationHelper();
        }


        [Fact]
        public async Task Activation_of_not_existing_counterparty_should_fail()
        {
            var context = _mockCreationHelper.GetContextMock();
            var counterpartyManagementService = _mockCreationHelper.GetCounterpartyManagementService(context.Object);

            var (_, isFailure, error) = await counterpartyManagementService.ActivateCounterparty(8, "Test reason");

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Activation_of_counterparty_with_empty_reason_should_fail()
        {
            var context = _mockCreationHelper.GetContextMock();
            var counterpartyManagementService = _mockCreationHelper.GetCounterpartyManagementService(context.Object);

            var (_, isFailure, error) = await counterpartyManagementService.ActivateCounterparty(2, null);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Activation_of_active_counterparty_should_succeed()
        {
            var context = _mockCreationHelper.GetContextMock().Object;
            var counterpartyManagementService = _mockCreationHelper.GetCounterpartyManagementService(context);

            var (_, isFailure, error) = await counterpartyManagementService.ActivateCounterparty(1, "Test reason");

            Assert.False(isFailure);
        }


        [Fact]
        public async Task Activation_of_counterparty_should_activate()
        {
            var context = _mockCreationHelper.GetContextMock().Object;
            var counterpartyManagementService = _mockCreationHelper.GetCounterpartyManagementService(context);

            var (_, isFailure, error) = await counterpartyManagementService.ActivateCounterparty(2, "Test reason");

            var counterparty = context.Counterparties.First(c => c.Id == 1);
            Assert.False(isFailure);
            Assert.True(counterparty.IsActive);
        }


        [Fact]
        public async Task Activation_of_counterparty_should_not_activate_agencies()
        {
            var context = _mockCreationHelper.GetContextMock().Object;
            var counterpartyManagementService = _mockCreationHelper.GetCounterpartyManagementService(context);

            var (_, isFailure, error) = await counterpartyManagementService.ActivateCounterparty(2, "Test reason");

            var agencies = context.Agencies.Where(ag => ag.CounterpartyId == 2).ToList();
            Assert.False(isFailure);
            Assert.DoesNotContain(agencies.Where(a => a.Id == 3 || a.Id == 5), ag => ag.IsActive);
        }


        [Fact]
        public async Task Activation_of_counterparty_should_activate_accounts()
        {
            var context = _mockCreationHelper.GetContextMock().Object;
            var counterpartyManagementService = _mockCreationHelper.GetCounterpartyManagementService(context);

            var (_, isFailure, error) = await counterpartyManagementService.ActivateCounterparty(2, "Test reason");

            var accounts = context.CounterpartyAccounts.Where(ac => ac.Id == 2).ToList();
            Assert.False(isFailure);
            Assert.True(accounts.All(ac => ac.IsActive));
        }


        [Fact]
        public async Task Activation_of_active_agency_should_succeed()
        {
            var context = _mockCreationHelper.GetContextMock().Object;
            var agencyManagementService = _mockCreationHelper.GetAgencyManagementService(context);

            var (_, isFailure, error) = await agencyManagementService.ActivateAgency(1, "Test reason");

            Assert.False(isFailure);
        }


        [Fact]
        public async Task Activation_of_not_existing_agency_should_fail()
        {
            var context = _mockCreationHelper.GetContextMock();
            var agencyManagementService = _mockCreationHelper.GetAgencyManagementService(context.Object);

            var (_, isFailure, error) = await agencyManagementService.ActivateAgency(105, "Test reason");

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Activation_of_agency_with_empty_reason_should_fail()
        {
            var context = _mockCreationHelper.GetContextMock();
            var agencyManagementService = _mockCreationHelper.GetAgencyManagementService(context.Object);

            var (_, isFailure, error) = await agencyManagementService.ActivateAgency(3, null);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Activation_of_agency_should_activate()
        {
            var context = _mockCreationHelper.GetContextMock().Object;
            var agencyManagementService = _mockCreationHelper.GetAgencyManagementService(context);

            var (_, isFailure, error) = await agencyManagementService.ActivateAgency(3, "Test reason");

            var agency = context.Agencies.Single(ag => ag.Id == 3);
            Assert.False(isFailure);
            Assert.True(agency.IsActive);
        }


        [Fact]
        public async Task Activation_of_agency_should_activate_agents_relations()
        {
            var context = _mockCreationHelper.GetContextMock().Object;
            var agencyManagementService = _mockCreationHelper.GetAgencyManagementService(context);

            var (_, isFailure, error) = await agencyManagementService.ActivateAgency(3, "Test reason");

            var agentIds = new List<int> { 5, 6 };
            var agentAgencyRelations = context.AgentAgencyRelations.Where(ag => agentIds.Contains(ag.AgentId)).ToList();
            Assert.False(isFailure);
            Assert.True(agentAgencyRelations.All(agr => agr.IsActive));
        }


        [Fact]
        public async Task Activation_of_agency_should_activate_accounts()
        {
            var context = _mockCreationHelper.GetContextMock().Object;
            var agencyManagementService = _mockCreationHelper.GetAgencyManagementService(context);

            var (_, isFailure, error) = await agencyManagementService.ActivateAgency(3, "Test reason");

            var account = context.AgencyAccounts.Single(p => p.Id == 2);
            Assert.False(isFailure);
            Assert.True(account.IsActive);
        }


        [Fact]
        public async Task Activation_of_agency_should_activate_child_agencies()
        {
            var context = _mockCreationHelper.GetContextMock().Object;
            var agencyManagementService = _mockCreationHelper.GetAgencyManagementService(context);

            var (_, isFailure, error) = await agencyManagementService.ActivateAgency(3, "Test reason.");

            var childAgency = context.Agencies.Single(ag => ag.Id == 5);
            Assert.False(isFailure);
            Assert.True(childAgency.IsActive);
        }


        [Fact]
        public async Task Activation_of_default_agency_should_not_activate_counterparty()
        {
            var context = _mockCreationHelper.GetContextMock().Object;
            var agencyManagementService = _mockCreationHelper.GetAgencyManagementService(context);

            var (_, isFailure, error) = await agencyManagementService.ActivateAgency(3, "Test reason");

            var counterparty = context.Counterparties.Single(c => c.Id == 2);
            Assert.False(isFailure);
            Assert.False(counterparty.IsActive);
        }


        private readonly AdministratorServicesMockCreationHelper _mockCreationHelper;

        public void Dispose() { }
    }
}