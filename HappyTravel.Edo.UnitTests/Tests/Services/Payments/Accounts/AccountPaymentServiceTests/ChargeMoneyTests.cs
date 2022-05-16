using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Mailing;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.Api.Services.Payments;
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
    public class ChargeMoneyTests : IDisposable
    {
        public ChargeMoneyTests()
        {
            var entityLockerMock = new Mock<IEntityLocker>();

            entityLockerMock.Setup(l => l.Acquire<It.IsAnyType>(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(Result.Success()));

            var edoContextMock = MockEdoContextFactory.Create();
            _mockedEdoContext = edoContextMock.Object;

            var accountPaymentProcessingService = new AccountPaymentProcessingService(
                _mockedEdoContext, entityLockerMock.Object, Mock.Of<IAccountBalanceAuditService>());

            _accountPaymentService = new AccountPaymentService(accountPaymentProcessingService, _mockedEdoContext,
                new DefaultDateTimeProvider(), Mock.Of<IBalanceManagementNotificationsService>(),
                Mock.Of<IBookingRecordManager>(), Mock.Of<IBookingDocumentsMailingService>());

            var strategy = new ExecutionStrategyMock();

            var dbFacade = new Mock<DatabaseFacade>(_mockedEdoContext);
            dbFacade.Setup(d => d.CreateExecutionStrategy()).Returns(strategy);
            edoContextMock.Setup(c => c.Database).Returns(dbFacade.Object);

            edoContextMock
                .Setup(c => c.Agencies)
                .Returns(DbSetMockProvider.GetDbSetMock(new List<Agency>
                {
                    new Agency
                    {
                        Id = 1,
                        Name = "Agency",
                        ParentId = null,
                    },
                }));

            edoContextMock
                .Setup(c => c.AgencyAccounts)
                .Returns(DbSetMockProvider.GetDbSetMock(new List<AgencyAccount>
                {
                    _account,
                }));

            edoContextMock
                .Setup(c => c.Detach(_account));

            edoContextMock
                .Setup(c => c.Payments)
                .Returns(DbSetMockProvider.GetDbSetMock(new List<Payment>()));
        }



        [Fact]
        public async Task Successful_charge_should_lower_balance()
        {
            var paymentService = CreatePaymentServiceWithMoneyAmount(new MoneyAmount(100, Currencies.USD));

            var (isSuccess, _, _, _) = await _accountPaymentService.Charge("ReferenceCode", _agent.ToApiCaller(), paymentService);

            Assert.True(isSuccess);
            Assert.Equal(900m, _account.Balance);
        }


        [Fact]
        public async Task Successful_charge_should_process_changes()
        {
            var paymentServiceMock = CreatePaymentServiceMock();

            var (isSuccess, _, _, _) = await _accountPaymentService.Charge("referenceCode", _agent.ToApiCaller(), paymentServiceMock.Object);

            Assert.True(isSuccess);
            paymentServiceMock.Verify(p => p.ProcessPaymentChanges(It.IsAny<Payment>()), Times.Once);

            Mock<IPaymentCallbackService> CreatePaymentServiceMock()
            {
                var paymentServiceMock = new Mock<IPaymentCallbackService>();
                paymentServiceMock.Setup(p => p.GetChargingAccountId(It.IsAny<string>()))
                    .ReturnsAsync(_account.Id);

                paymentServiceMock.Setup(p => p.GetServiceBuyer(It.IsAny<string>()))
                    .ReturnsAsync((_agent.AgencyId, _agent.AgentId));

                paymentServiceMock.Setup(p => p.GetChargingAmount(It.IsAny<string>()))
                    .ReturnsAsync(new MoneyAmount(100, Currencies.USD));

                paymentServiceMock.Setup(p => p.ProcessPaymentChanges(It.IsAny<Payment>()));

                return paymentServiceMock;
            }
        }


        [Fact]
        public async Task Successful_charge_should_set_payment_status()
        {
            var paymentService = CreatePaymentServiceWithMoneyAmount(new MoneyAmount(100, Currencies.USD));

            var (isSuccess, _, _, _) = await _accountPaymentService.Charge("ReferenceCode", _agent.ToApiCaller(), paymentService);

            Assert.True(isSuccess);
            Assert.True(_mockedEdoContext.Payments.Any());
            Assert.Equal(PaymentStatuses.Captured, _mockedEdoContext.Payments.First().Status);
        }


        [Fact]
        public async Task Charge_from_account_with_balance_more_allowed_overdraft_should_succeed()
        {
            _account.Balance = 25m;
            var paymentService = CreatePaymentServiceWithMoneyAmount(new MoneyAmount(100, Currencies.USD));

            var (isSuccess, _, _, _) = await _accountPaymentService.Charge("ReferenceCode", _agent.ToApiCaller(), paymentService);

            Assert.True(isSuccess);
            Assert.Equal(-75m, _account.Balance);
        }


        [Fact]
        public async Task Charge_from_account_with_insufficient_overdraft_should_fail()
        {
            var startingBalance = _account.Balance = 24m;
            var paymentService = CreatePaymentServiceWithMoneyAmount(new MoneyAmount(100, Currencies.USD));

            var (_, isFailure, _, _) = await _accountPaymentService.Charge("ReferenceCode", _agent.ToApiCaller(), paymentService);

            Assert.True(isFailure);
            Assert.Equal(startingBalance, _account.Balance);
        }


        [Fact]
        public async Task Charge_from_nonexistent_booking_should_fail()
        {
            var paymentService = CreatePaymentServiceWithoutServicePrice();

            var (_, isFailure, _, _) = await _accountPaymentService.Charge("REFCODE" + " wrong code", _agent.ToApiCaller(), paymentService);

            Assert.True(isFailure);

            IPaymentCallbackService CreatePaymentServiceWithoutServicePrice()
            {
                var paymentServiceMock = new Mock<IPaymentCallbackService>();

                paymentServiceMock.Setup(p => p.GetChargingAmount(It.IsAny<string>()))
                    .ReturnsAsync(Result.Failure<MoneyAmount>("Could not get service price"));

                return paymentServiceMock.Object;
            }
        }


        [Fact]
        public async Task Charge_when_no_account_with_booking_currency_should_fail()
        {
            var paymentService = CreatePaymentServiceWithoutAgencyAccount();

            var (_, isFailure, _, _) = await _accountPaymentService.Charge("referenceCode", _agent.ToApiCaller(), paymentService);

            Assert.True(isFailure);


            IPaymentCallbackService CreatePaymentServiceWithoutAgencyAccount()
            {
                var paymentServiceMock = new Mock<IPaymentCallbackService>();

                paymentServiceMock.Setup(p => p.GetChargingAmount(It.IsAny<string>()))
                    .ReturnsAsync(new MoneyAmount());

                paymentServiceMock.Setup(p => p.GetChargingAccountId(It.IsAny<string>()))
                    .ReturnsAsync(Result.Failure<int>("Could not get agency account"));

                return paymentServiceMock.Object;
            }
        }


        private IPaymentCallbackService CreatePaymentServiceWithMoneyAmount(MoneyAmount moneyAmount)
        {
            var paymentServiceMock = new Mock<IPaymentCallbackService>();
            paymentServiceMock.Setup(p => p.GetChargingAccountId(It.IsAny<string>()))
                .ReturnsAsync(_account.Id);

            paymentServiceMock.Setup(p => p.GetServiceBuyer(It.IsAny<string>()))
                .ReturnsAsync((_agent.AgencyId, _agent.AgentId));

            paymentServiceMock.Setup(p => p.GetChargingAmount(It.IsAny<string>()))
                .ReturnsAsync(moneyAmount);

            paymentServiceMock.Setup(p => p.ProcessPaymentChanges(It.IsAny<Payment>()));

            return paymentServiceMock.Object;
        }


        public void Dispose()
        {

        }


        private readonly AgencyAccount _account = new()
        {
            Id = 1,
            Balance = 1000,
            Currency = Currencies.USD,
            AgencyId = 1,
            IsActive = true
        };


        private readonly EdoContext _mockedEdoContext;
        private readonly AccountPaymentService _accountPaymentService;
        private readonly AgentContext _agent =
            new(1, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
                1, string.Empty, true, InAgencyPermissions.All, string.Empty, string.Empty,
                string.Empty, 1, new(), ContractKind.VirtualAccountOrCreditCardPayments);
    }
}
