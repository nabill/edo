using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Edo.UnitTests.Utility;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.AdministratorServices.AgencyVerificationServiceTests
{
    public class AgencyVerificationTests : IDisposable
    {
        public AgencyVerificationTests()
        {
            _administratorServicesMockCreationHelper = new AdministratorServicesMockCreationHelper();
        }


        [Fact]
        public async Task Verification_of_not_existing_agency_as_full_accessed_should_fail()
        {
            var context = _administratorServicesMockCreationHelper.GetContextMock().Object;
            var agencyVerificationService = _administratorServicesMockCreationHelper.GetAgencyVerificationService(context);

            var (_, isFailure, error) = await agencyVerificationService.VerifyAsFullyAccessed(7, ContractKind.CashPayments, "Test reason");

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Verification_of_not_existing_agency_as_read_only_should_fail()
        {
            var context = _administratorServicesMockCreationHelper.GetContextMock().Object;
            var agencyVerificationService = _administratorServicesMockCreationHelper.GetAgencyVerificationService(context);

            var (_, isFailure, error) = await agencyVerificationService.VerifyAsReadOnly(7, "Test reason");

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Verification_as_full_accessed_should_update_agency_state()
        {
            var context = _administratorServicesMockCreationHelper.GetContextMock().Object;
            var agencyVerificationService = _administratorServicesMockCreationHelper.GetAgencyVerificationService(context);

            var (_, isFailure, _) = await agencyVerificationService.VerifyAsFullyAccessed(20, ContractKind.CashPayments, "Test reason");

            var agency = context.Agencies.Single(c => c.Id == 20);
            Assert.False(isFailure);
            Assert.True(agency.VerificationState == AgencyVerificationStates.FullAccess && agency.VerificationReason.Contains("Test reason"));
        }
        
        
        [Fact]
        public async Task Verification_as_full_accessed_for_not_verified_read_only_should_fail()
        {
            var context = _administratorServicesMockCreationHelper.GetContextMock().Object;
            var agencyVerificationService = _administratorServicesMockCreationHelper.GetAgencyVerificationService(context);

            var (_, isFailure, _) = await agencyVerificationService.VerifyAsFullyAccessed(3, ContractKind.CashPayments, "Test reason");

            var agency = context.Agencies.Single(c => c.Id == 3);
            Assert.True(isFailure);
            Assert.True(agency.VerificationState == AgencyVerificationStates.PendingVerification);
        }
        
        
        [Fact]
        public async Task Verification_as_read_only_for_full_accessed_agency_should_fail()
        {
            var context = _administratorServicesMockCreationHelper.GetContextMock().Object;
            var agencyVerificationService = _administratorServicesMockCreationHelper.GetAgencyVerificationService(context);

            var (_, isFailure, _) = await agencyVerificationService.VerifyAsReadOnly(14, "Test reason");

            var agency = context.Agencies.Single(c => c.Id == 14);
            Assert.True(isFailure);
            Assert.True(agency.VerificationState == AgencyVerificationStates.FullAccess);
        }


        [Fact]
        public async Task Verification_as_read_only_should_update_agency_state()
        {
            var context = _administratorServicesMockCreationHelper.GetContextMock().Object;
            var agencyVerificationService = _administratorServicesMockCreationHelper.GetAgencyVerificationService(context);

            var (_, isFailure, error) = await agencyVerificationService.VerifyAsReadOnly(1, "Test reason");

            var agency = context.Agencies.Single(c => c.Id == 1);
            Assert.False(isFailure);
            Assert.True(agency.VerificationState == AgencyVerificationStates.ReadOnly && agency.VerificationReason.Contains("Test reason"));
        }


        [Fact]
        public async Task Verification_as_read_only_should_update_accounts()
        {
            var context = _administratorServicesMockCreationHelper.GetContextMock().Object;
            var agencyVerificationService = _administratorServicesMockCreationHelper.GetAgencyVerificationService(context);

            var (_, isFailure, error) = await agencyVerificationService.VerifyAsReadOnly(1, "Test reason");

            var agencies = new List<int> {1, 2, 4};
            Assert.False(isFailure);
            Assert.Equal(5, context.AgencyAccounts.ToList().Count);
            Assert.True(agencies.All(a => context.AgencyAccounts.Any(ac => ac.AgencyId == a)));
        }
        
        
        [Fact]
        public async Task Full_access_verification_with_empty_contract_type_must_fail()
        {
            var context = _administratorServicesMockCreationHelper.GetContextMock().Object;
            var agencyVerificationService = _administratorServicesMockCreationHelper.GetAgencyVerificationService(context);

            var (_, isFailure, _) = await agencyVerificationService.VerifyAsFullyAccessed(14, default, "Test reason");

            Assert.True(isFailure);
        }


        [Theory]
        [InlineData(ContractKind.CashPayments)]
        [InlineData(ContractKind.CreditPayments)]
        public async Task Full_access_verification_must_set_contract_type(ContractKind contractKind)
        {
            var context = _administratorServicesMockCreationHelper.GetContextMock().Object;
            var agencyVerificationService = _administratorServicesMockCreationHelper.GetAgencyVerificationService(context);

            await agencyVerificationService.VerifyAsFullyAccessed(20, contractKind, "Test reason");

            Assert.Equal(contractKind, context.Agencies.Single(c => c.Id == 20).ContractKind);
        }


        private readonly AdministratorServicesMockCreationHelper _administratorServicesMockCreationHelper;

        public void Dispose() { }
    }
}