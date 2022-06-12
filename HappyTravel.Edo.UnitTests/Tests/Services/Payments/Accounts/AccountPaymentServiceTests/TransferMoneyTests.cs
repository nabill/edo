using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Mailing;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Payments.Accounts;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Edo.Data.Payments;
using HappyTravel.Edo.UnitTests.Mocks;
using HappyTravel.Edo.UnitTests.Utility;
using HappyTravel.Money.Enums;
using HappyTravel.Money.Models;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Moq;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Payments.Accounts.AccountPaymentServiceTests
{
    public class TransferMoneyTests : IDisposable
    {
        [Fact]
        public async Task Nonexistent_payer_account_should_fail()
        {
            var edoContext = CreateEdoContext();
            var accountPaymentService = GetPaymentService(edoContext);
            var (_, isFailure, _) = await accountPaymentService.TransferToChildAgency(0, 2, new MoneyAmount(1m, Currencies.USD), GetAgentForAgency(1));

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Nonexistent_recipient_account_should_fail()
        {
            var edoContext = CreateEdoContext();
            var accountPaymentService = GetPaymentService(edoContext);
            var (_, isFailure, _) = await accountPaymentService.TransferToChildAgency(1, 0, new MoneyAmount(1m, Currencies.USD), GetAgentForAgency(1));

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Agent_transfer_from_different_agency_should_fail()
        {
            var edoContext = CreateEdoContext();
            var accountPaymentService = GetPaymentService(edoContext);
            var (_, isFailure, _) = await accountPaymentService.TransferToChildAgency(1, 2, new MoneyAmount(1m, Currencies.USD), GetAgentForAgency(2));

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Negative_amount_transfer_should_fail()
        {
            var edoContext = CreateEdoContext();
            var accountPaymentService = GetPaymentService(edoContext);
            var (_, isFailure, _) = await accountPaymentService.TransferToChildAgency(1, 2, new MoneyAmount(-1m, Currencies.USD), GetAgentForAgency(1));

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Transfer_to_not_child_agency_should_fail()
        {
            var edoContext = CreateEdoContext();
            var accountPaymentService = GetPaymentService(edoContext);
            var (_, isFailure, _) = await accountPaymentService.TransferToChildAgency(1, 3, new MoneyAmount(1m, Currencies.USD), GetAgentForAgency(1));

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Transfer_to_account_with_different_currency_should_fail()
        {
            var edoContext = CreateEdoContext();
            var accountPaymentService = GetPaymentService(edoContext);
            var (_, isFailure, _) = await accountPaymentService.TransferToChildAgency(1, 4, new MoneyAmount(1m, Currencies.USD), GetAgentForAgency(1));

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Transfer_amount_with_different_currency_should_fail()
        {
            var edoContext = CreateEdoContext();
            var accountPaymentService = GetPaymentService(edoContext);
            var (_, isFailure, _) = await accountPaymentService.TransferToChildAgency(1, 2, new MoneyAmount(1m, Currencies.EUR), GetAgentForAgency(1));

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Transfer_when_balance_insufficient_should_fail()
        {
            var edoContext = CreateEdoContext();
            var accountPaymentService = GetPaymentService(edoContext);
            var (_, isFailure, _) = await accountPaymentService.TransferToChildAgency(1, 2, new MoneyAmount(1000000m, Currencies.USD), GetAgentForAgency(1));

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Correct_transfer_should_succeed()
        {
            var edoContext = CreateEdoContext();
            var accountPaymentService = GetPaymentService(edoContext);
            var (isSuccess, _, _) = await accountPaymentService.TransferToChildAgency(1, 2, new MoneyAmount(1m, Currencies.USD), GetAgentForAgency(1));

            Assert.True(isSuccess);
        }


        [Fact]
        public async Task Correct_transfer_should_subtract_correct_value()
        {
            var edoContext = CreateEdoContext();
            var accountPaymentService = GetPaymentService(edoContext);
            var payerAccount = edoContext.AgencyAccounts.Single(a => a.Id == 1);
            var (isSuccess, _, _) = await accountPaymentService.TransferToChildAgency(1, 2, new MoneyAmount(1m, Currencies.USD), GetAgentForAgency(1));

            Assert.True(isSuccess);
            Assert.Equal(999m, payerAccount.Balance);
        }


        [Fact]
        public async Task Correct_transfer_should_add_correct_value()
        {
            var edoContext = CreateEdoContext();
            var accountPaymentService = GetPaymentService(edoContext);
            var recipientAccount = edoContext.AgencyAccounts.Single(a => a.Id == 2);
            var (isSuccess, _, _) = await accountPaymentService.TransferToChildAgency(1, 2, new MoneyAmount(1m, Currencies.USD), GetAgentForAgency(1));

            Assert.True(isSuccess);
            Assert.Equal(1001m, recipientAccount.Balance);
        }


        private static EdoContext CreateEdoContext()
        {
            var edoContextMock = MockEdoContextFactory.Create();
            var strategy = new ExecutionStrategyMock();
            var dbFacade = new Mock<DatabaseFacade>(edoContextMock.Object);

            dbFacade
                .Setup(d => d.CreateExecutionStrategy())
                .Returns(strategy);
            
            edoContextMock
                .Setup(c => c.Database)
                .Returns(dbFacade.Object);

            edoContextMock
                .Setup(c => c.Agencies)
                .Returns(DbSetMockProvider.GetDbSetMock(new List<Agency>
                {
                    new()
                    {
                        Id = 1,
                        Name = "RootAgency",
                        ParentId = null,
                    },
                    new()
                    {
                        Id = 2,
                        Name = "ChildAgency",
                        ParentId = 1,
                    },
                    new()
                    {
                        Id = 3,
                        Name = "UnrelatedAgency",
                        ParentId = null,
                    },
                }));

            edoContextMock
                .Setup(c => c.AgencyAccounts)
                .Returns(DbSetMockProvider.GetDbSetMock(new List<AgencyAccount>
                {
                    new()
                    {
                        Id = 1,
                        Balance = 1000,
                        Currency = Currencies.USD,
                        AgencyId = 1,
                        IsActive = true
                    },
                    new()
                    {
                        Id = 2,
                        Balance = 1000,
                        Currency = Currencies.USD,
                        AgencyId = 2,
                        IsActive = true
                    },
                    new()
                    {
                        Id = 3,
                        Balance = 1000,
                        Currency = Currencies.USD,
                        AgencyId = 3,
                        IsActive = true
                    },
                    new()
                    {
                        Id = 4,
                        Balance = 1000,
                        Currency = Currencies.EUR,
                        AgencyId = 2,
                        IsActive = true
                    },
                }));

            return edoContextMock.Object;
        }


        private static IAccountPaymentService GetPaymentService(EdoContext edoContextMock)
        {
            var entityLockerMock = new Mock<IEntityLocker>();
            var accountPaymentProcessingService = new AccountPaymentProcessingService(
                edoContextMock, entityLockerMock.Object, Mock.Of<IAccountBalanceAuditService>());
            
            entityLockerMock
                .Setup(l => l.Acquire<It.IsAnyType>(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(Result.Success()));
            
            return new AccountPaymentService(accountPaymentProcessingService, edoContextMock,
                Mock.Of<IDateTimeProvider>(), Mock.Of<IBalanceManagementNotificationsService>(),
                Mock.Of<IBookingRecordManager>(), Mock.Of<IBookingDocumentsMailingService>());
        }


        private static AgentContext GetAgentForAgency(int agencyId)
        {
            return new AgentContext(1, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
                agencyId, string.Empty, true, InAgencyPermissions.All, string.Empty, string.Empty, string.Empty,
                1, new List<int>(), ContractKind.VirtualAccountOrCreditCardPayments);
        }

        public void Dispose()
        {

        }
    }
}
