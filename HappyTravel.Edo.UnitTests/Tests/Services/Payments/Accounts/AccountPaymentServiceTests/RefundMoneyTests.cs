using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Mailing;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Payments;
using HappyTravel.Edo.Api.Services.Payments.Accounts;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Edo.Data.Bookings;
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
    public class RefundMoneyTests : IDisposable
    {
        public RefundMoneyTests()
        {
            var entityLockerMock = new Mock<IEntityLocker>();

            entityLockerMock.Setup(l => l.Acquire<It.IsAnyType>(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(Result.Success()));

            var edoContextMock = MockEdoContextFactory.Create();

            edoContextMock
                .Setup(c => c.Agents)
                .Returns(DbSetMockProvider.GetDbSetMock(new List<Agent>
                {
                    new Agent
                    {
                        Id = 1,
                        Email = "email"
                    },
                }));

            var edoContextMock1 = edoContextMock;
            var mockedEdoContext = edoContextMock.Object;

            var accountPaymentProcessingService = new AccountPaymentProcessingService(
                mockedEdoContext, entityLockerMock.Object, Mock.Of<IAccountBalanceAuditService>(), Mock.Of<IAgentContextService>());

            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            dateTimeProviderMock.Setup(d => d.UtcNow()).Returns(CancellationDate);

            var bookingRecordManagerMock = new Mock<IBookingRecordManager>();
            bookingRecordManagerMock.Setup(b => b.Get(It.IsAny<string>()))
                .ReturnsAsync(Booking);

            _accountPaymentService = new AccountPaymentService(accountPaymentProcessingService, mockedEdoContext,
                dateTimeProviderMock.Object, Mock.Of<IBalanceManagementNotificationsService>(),
                bookingRecordManagerMock.Object, Mock.Of<IBookingDocumentsMailingService>(), Mock.Of<IAgentContextService>());

            var strategy = new ExecutionStrategyMock();

            var dbFacade = new Mock<DatabaseFacade>(mockedEdoContext);
            dbFacade.Setup(d => d.CreateExecutionStrategy()).Returns(strategy);
            edoContextMock.Setup(c => c.Database).Returns(dbFacade.Object);

            edoContextMock1
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

            edoContextMock1
                .Setup(c => c.AgencyAccounts)
                .Returns(DbSetMockProvider.GetDbSetMock(new List<AgencyAccount>
                {
                    _account,
                }));


            edoContextMock1
                .Setup(c => c.Payments)
                .Returns(DbSetMockProvider.GetDbSetMock(new List<Payment>
                {
                    _payment,
                }));
        }


        [Fact]
        public async Task Successful_refund_should_rise_balance()
        {
            var paymentService = CreatePaymentServiceWithRefundableAmount(new MoneyAmount(100, Currencies.USD));

            var (isSuccess, _, _) = await _accountPaymentService.Refund("ReferenceCode", CancellationDate, paymentService, "reason");

            Assert.True(isSuccess);
            Assert.Equal(1100m, _account.Balance);
        }


        [Fact]
        public async Task Successful_refund_should_change_payment_status()
        {
            var paymentService = CreatePaymentServiceWithRefundableAmount(new MoneyAmount(100, Currencies.USD));

            var (isSuccess, _, error) = await _accountPaymentService.Refund("ReferenceCode", CancellationDate, paymentService, "reason");

            Assert.True(isSuccess);
            Assert.Equal(PaymentStatuses.Refunded, _payment.Status);
        }


        [Fact]
        public async Task Refund_booking_with_not_existing_agency_account_should_fail()
        {
            var paymentService = CreatePaymentServiceWithInvalidAccount();

            var (_, isFailure, _) = await _accountPaymentService.Refund("ReferenceCode", CancellationDate, paymentService, "reason");

            Assert.True(isFailure);
            Assert.Equal(1000m, _account.Balance);


            IPaymentCallbackService CreatePaymentServiceWithInvalidAccount()
            {
                var paymentServiceMock = new Mock<IPaymentCallbackService>();
                paymentServiceMock.Setup(p => p.GetChargingAccountId(It.IsAny<string>()))
                    .ReturnsAsync(Result.Failure<int>("Could not get agency account"));

                paymentServiceMock.Setup(p => p.GetServiceBuyer(It.IsAny<string>()))
                    .ReturnsAsync((_agent.AgencyId, _agent.AgentId));

                paymentServiceMock.Setup(p => p.GetRefundableAmount(It.IsAny<string>(), It.IsAny<DateTimeOffset>()))
                    .ReturnsAsync(new MoneyAmount());

                paymentServiceMock.Setup(p => p.ProcessPaymentChanges(It.IsAny<Payment>()));

                return paymentServiceMock.Object;
            }
        }


        [Fact]
        public async Task Refund_booking_without_payment_should_fail()
        {
            var paymentService = CreatePaymentServiceWithRefundableAmount(new MoneyAmount(100, Currencies.USD));

            var (_, isFailure, _) = await _accountPaymentService.Refund("InvalidReferenceCode", CancellationDate, paymentService, "reason");

            Assert.True(isFailure);
            Assert.Equal(1000m, _account.Balance);
        }



        private IPaymentCallbackService CreatePaymentServiceWithRefundableAmount(MoneyAmount moneyAmount)
        {
            var paymentServiceMock = new Mock<IPaymentCallbackService>();
            paymentServiceMock.Setup(p => p.GetChargingAccountId(It.IsAny<string>()))
                .ReturnsAsync(_account.Id);

            paymentServiceMock.Setup(p => p.GetServiceBuyer(It.IsAny<string>()))
                .ReturnsAsync((_agent.AgencyId, _agent.AgentId));

            paymentServiceMock.Setup(p => p.GetRefundableAmount(It.IsAny<string>(), It.IsAny<DateTimeOffset>()))
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

        private readonly Payment _payment = new()
        {
            Id = 1,
            Amount = 100,
            Status = PaymentStatuses.Captured,
            ReferenceCode = "ReferenceCode"
        };

        private static readonly DateTimeOffset CancellationDate = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly Booking Booking = new Booking
        {
            AgentId = 1,
            ReferenceCode = "ReferenceCode"
        };

        private readonly AccountPaymentService _accountPaymentService;
        private readonly AgentContext _agent =
            new(1, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
                1, string.Empty, true, InAgencyPermissions.All, string.Empty, string.Empty,
                string.Empty, 1, new(), ContractKind.VirtualAccountOrCreditCardPayments);
    }
}
