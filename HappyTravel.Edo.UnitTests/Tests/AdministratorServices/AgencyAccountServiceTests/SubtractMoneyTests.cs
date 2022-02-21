using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.AdministratorServices;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Api.Services.Payments.Accounts;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Edo.Data.Payments;
using HappyTravel.Edo.UnitTests.Mocks;
using HappyTravel.Edo.UnitTests.Utility;
using HappyTravel.Money.Enums;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Moq;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.AdministratorServices.AgencyAccountServiceTests
{
    public class SubtractMoneyTests
    {
        public SubtractMoneyTests()
        {
            var entityLockerMock = new Mock<IEntityLocker>();

            entityLockerMock.Setup(l => l.Acquire<It.IsAnyType>(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(Result.Success()));

            var edoContextMock = MockEdoContextFactory.Create();
            _edoContextMock = edoContextMock;
            _mockedEdoContext = edoContextMock.Object;

            _agencyAccountService = new AgencyAccountService(_mockedEdoContext, entityLockerMock.Object, 
                Mock.Of<IManagementAuditService>(), Mock.Of<IAccountBalanceAuditService>());

            var strategy = new ExecutionStrategyMock();

            var dbFacade = new Mock<DatabaseFacade>(_mockedEdoContext);
            dbFacade.Setup(d => d.CreateExecutionStrategy()).Returns(strategy);
            edoContextMock.Setup(c => c.Database).Returns(dbFacade.Object);

        }

        [Fact]
        public async Task Subtract_money_with_currency_mismatch_should_fail()
        {
            SetupInitialData();

            var (_, isFailure, error) = await _agencyAccountService.Subtract(
                1, new PaymentData(1, Currencies.EUR, "test"), _apiCaller);
            
            Assert.True(isFailure);
        }

        [Fact]
        public async Task Subtract_money_with_negative_amount_should_fail()
        {
            SetupInitialData();

            var (_, isFailure, error) = await _agencyAccountService.Subtract(
                1, new PaymentData(-1, Currencies.USD, "test"), _apiCaller);
            
            Assert.True(isFailure);
        }

        [Fact]
        public async Task Subtract_money_from_suitable_account_should_decrease_balance()
        {
            SetupInitialData();
            var affectedAccount = _mockedEdoContext.AgencyAccounts.Single(a => a.Id == 1);

            var (isSuccess, _, error) = await _agencyAccountService.Subtract(
                1, new PaymentData(1, Currencies.USD, "test"), _apiCaller);

            Assert.True(isSuccess);
            Assert.Equal(999, affectedAccount.Balance);
        }


        private void SetupInitialData()
        {
            _edoContextMock
                .Setup(c => c.Agencies)
                .Returns(DbSetMockProvider.GetDbSetMock(new List<Agency>
                {
                    new Agency
                    {
                        Id = 1,
                        ParentId = null,
                    },
                    new Agency
                    {
                        Id = 2,
                        ParentId = null,
                    }
                }));

            _edoContextMock
                .Setup(c => c.AgencyAccounts)
                .Returns(DbSetMockProvider.GetDbSetMock(new List<AgencyAccount>
                {
                    new AgencyAccount
                    {
                        Id = 1,
                        Balance = 1000,
                        Currency = Currencies.USD,
                        AgencyId = 2,
                        IsActive = true
                    },
                    new AgencyAccount
                    {
                        Id = 2,
                        Balance = 1000,
                        Currency = Currencies.USD,
                        AgencyId = 2,
                        IsActive = true
                    }
                }));
        }

        private readonly Mock<EdoContext> _edoContextMock;
        private readonly EdoContext _mockedEdoContext;
        private readonly ApiCaller _apiCaller = new ApiCaller(1.ToString(), ApiCallerTypes.Admin);
        private readonly IAgencyAccountService _agencyAccountService;
    }
}
