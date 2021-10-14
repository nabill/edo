using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.AdministratorServices;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Api.Services.Payments.Accounts;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Edo.Data.Payments;
using HappyTravel.Edo.UnitTests.Mocks;
using HappyTravel.Edo.UnitTests.Utility;
using HappyTravel.Money.Enums;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Moq;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.AdministratorServices.CounterpartyAccountServiceTests
{
    public class GetBalanceTests
    {
        public GetBalanceTests()
        {
            var entityLockerMock = new Mock<IEntityLocker>();

            entityLockerMock.Setup(l => l.Acquire<It.IsAnyType>(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(Result.Success()));

            var edoContextMock = MockEdoContextFactory.Create();
            var mockedEdoContext = edoContextMock.Object;

            _agencyAccountService = new AgencyAccountService(mockedEdoContext, entityLockerMock.Object, 
                Mock.Of<IManagementAuditService>(), Mock.Of<IAccountBalanceAuditService>());

            var strategy = new ExecutionStrategyMock();

            var dbFacade = new Mock<DatabaseFacade>(mockedEdoContext);
            dbFacade.Setup(d => d.CreateExecutionStrategy()).Returns(strategy);
            edoContextMock.Setup(c => c.Database).Returns(dbFacade.Object);

            edoContextMock
                .Setup(c => c.Agencies)
                .Returns(DbSetMockProvider.GetDbSetMock(new List<Agency>
                {
                    new Agency
                    {
                        Id = 1
                    },
                    // Having more than one element for predicates to be tested too
                    new Agency
                    {
                        Id = 2
                    },
                }));
            
            edoContextMock
                .Setup(c => c.AgencyAccounts)
                .Returns(DbSetMockProvider.GetDbSetMock(new List<AgencyAccount>
                {
                    new AgencyAccount
                    {
                        Id = 1,
                        Balance = 1000,
                        Currency = Currencies.USD,
                        AgencyId = 1,
                        IsActive = true
                    },
                    new AgencyAccount
                    {
                        Id = 1,
                        Balance = 0,
                        Currency = Currencies.USD,
                        AgencyId = 1,
                        IsActive = true
                    }
                }));
        }


        [Fact]
        public async Task Existing_currency_balance_should_be_shown()
        {
            var balanceInfo = await _agencyAccountService.Get(1, Currencies.USD);
            
            Assert.Equal(1000, balanceInfo[0].Balance.Amount);
        }


        [Fact]
        public async Task Not_existing_currency_balance_show_should_empty_list()
        {
            var balanceInfo = await _agencyAccountService.Get(1, Currencies.EUR);
            
            Assert.True(balanceInfo.Count == 0);
        }


        private readonly IAgencyAccountService _agencyAccountService;
    }
}
