using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Edo.UnitTests.Utility;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.AdministratorServices.CounterpartyManagementServiceTests
{
    public class CounterpartyVerificationTests : IDisposable
    {
        public CounterpartyVerificationTests()
        {
            _administratorServicesMockCreationHelper = new AdministratorServicesMockCreationHelper();
        }


        [Fact]
        public async Task Verification_of_not_existing_counterparty_as_full_accessed_should_fail()
        {
            var context = _administratorServicesMockCreationHelper.GetContextMock().Object;
            var counterpartyVerificationService = _administratorServicesMockCreationHelper.GetCounterpartyVerificationService(context);

            var (_, isFailure, error) = await counterpartyVerificationService.VerifyAsFullyAccessed(7, CounterpartyContractKind.CashPayments, "Test reason");

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Verification_of_not_existing_counterparty_as_read_only_should_fail()
        {
            var context = _administratorServicesMockCreationHelper.GetContextMock().Object;
            var counterpartyVerificationService = _administratorServicesMockCreationHelper.GetCounterpartyVerificationService(context);

            var (_, isFailure, error) = await counterpartyVerificationService.VerifyAsReadOnly(7, "Test reason");

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Verification_as_full_accessed_should_update_counterparty_state()
        {
            var context = _administratorServicesMockCreationHelper.GetContextMock().Object;
            var counterpartyVerificationService = _administratorServicesMockCreationHelper.GetCounterpartyVerificationService(context);

            var (_, isFailure, _) = await counterpartyVerificationService.VerifyAsFullyAccessed(3, CounterpartyContractKind.CashPayments, "Test reason");

            var counterparty = context.Counterparties.Single(c => c.Id == 3);
            Assert.False(isFailure);
            Assert.True(counterparty.State == CounterpartyStates.FullAccess && counterparty.VerificationReason.Contains("Test reason"));
        }
        
        
        [Fact]
        public async Task Verification_as_full_accessed_for_not_verified_read_only_should_fail()
        {
            var context = _administratorServicesMockCreationHelper.GetContextMock().Object;
            var counterpartyVerificationService = _administratorServicesMockCreationHelper.GetCounterpartyVerificationService(context);

            var (_, isFailure, _) = await counterpartyVerificationService.VerifyAsFullyAccessed(2, CounterpartyContractKind.CashPayments, "Test reason");

            var counterparty = context.Counterparties.Single(c => c.Id == 2);
            Assert.True(isFailure);
            Assert.True(counterparty.State == CounterpartyStates.PendingVerification);
        }
        
        
        [Fact]
        public async Task Verification_as_read_only_for_full_accessed_counterparty_should_fail()
        {
            var context = _administratorServicesMockCreationHelper.GetContextMock().Object;
            var counterpartyVerificationService = _administratorServicesMockCreationHelper.GetCounterpartyVerificationService(context);

            var (_, isFailure, _) = await counterpartyVerificationService.VerifyAsReadOnly(14, "Test reason");

            var counterparty = context.Counterparties.Single(c => c.Id == 14);
            Assert.True(isFailure);
            Assert.True(counterparty.State == CounterpartyStates.FullAccess);
        }


        [Fact]
        public async Task Verification_as_read_only_should_update_counterparty_state()
        {
            var context = _administratorServicesMockCreationHelper.GetContextMock().Object;
            var counterpartyVerificationService = _administratorServicesMockCreationHelper.GetCounterpartyVerificationService(context);

            var (_, isFailure, error) = await counterpartyVerificationService.VerifyAsReadOnly(1, "Test reason");

            var counterparty = context.Counterparties.Single(c => c.Id == 1);
            Assert.False(isFailure);
            Assert.True(counterparty.State == CounterpartyStates.ReadOnly && counterparty.VerificationReason.Contains("Test reason"));
        }


        [Fact]
        public async Task Verification_as_read_only_should_update_accounts()
        {
            var context = _administratorServicesMockCreationHelper.GetContextMock().Object;
            var counterpartyVerificationService = _administratorServicesMockCreationHelper.GetCounterpartyVerificationService(context);

            var (_, isFailure, error) = await counterpartyVerificationService.VerifyAsReadOnly(1, "Test reason");

            var agencies = new List<int>() {1, 2};
            Assert.False(isFailure);
            Assert.Equal(3, context.CounterpartyAccounts.ToList().Count);
            Assert.True(agencies.All(a => context.AgencyAccounts.Any(ac => ac.AgencyId == a)));
        }
        
        
        [Fact]
        public async Task Full_access_verification_with_empty_contract_type_must_fail()
        {
            var context = _administratorServicesMockCreationHelper.GetContextMock().Object;
            var counterpartyVerificationService = _administratorServicesMockCreationHelper.GetCounterpartyVerificationService(context);

            var (_, isFailure, _) = await counterpartyVerificationService.VerifyAsFullyAccessed(14, default, "Test reason");

            Assert.True(isFailure);
        }


        [Theory]
        [InlineData(CounterpartyContractKind.CashPayments)]
        [InlineData(CounterpartyContractKind.CreditPayments)]
        public async Task Full_access_verification_must_set_contract_type(CounterpartyContractKind contractKind)
        {
            var context = _administratorServicesMockCreationHelper.GetContextMock().Object;
            var counterpartyVerificationService = _administratorServicesMockCreationHelper.GetCounterpartyVerificationService(context);

            await counterpartyVerificationService.VerifyAsFullyAccessed(3, contractKind, "Test reason");

            Assert.Equal(contractKind, context.Counterparties.Single(c => c.Id == 3).ContractKind);
        }


        private readonly AdministratorServicesMockCreationHelper _administratorServicesMockCreationHelper;

        public void Dispose() { }
    }
}