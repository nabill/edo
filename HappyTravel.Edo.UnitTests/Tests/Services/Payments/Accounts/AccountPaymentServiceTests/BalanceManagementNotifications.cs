using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Mailing;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.Api.Services.Agents;
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
    public class BalanceManagementNotifications : IDisposable
    {
        public BalanceManagementNotifications()
        {
            var entityLockerMock = new Mock<IEntityLocker>();

            entityLockerMock.Setup(l => l.Acquire<It.IsAnyType>(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(Result.Success()));

            var edoContextMock = MockEdoContextFactory.Create();
            _mockedEdoContext = edoContextMock.Object;

            var accountPaymentProcessingService = new AccountPaymentProcessingService(
                _mockedEdoContext, entityLockerMock.Object, Mock.Of<IAccountBalanceAuditService>());

            _balanceManagementNotificationsServiceMock = new Mock<IBalanceManagementNotificationsService>();
            _accountPaymentService = new AccountPaymentService(accountPaymentProcessingService, _mockedEdoContext,
                new DefaultDateTimeProvider(), _balanceManagementNotificationsServiceMock.Object, Mock.Of<IBookingRecordManager>(),
                Mock.Of<IBookingDocumentsMailingService>());

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
        public async Task Successful_charge_should_call_notification_with_correct_parameters()
        {
            var chargingAmount = new MoneyAmount(100, Currencies.USD);
            var paymentService = CreatePaymentServiceWithMoneyAmount(chargingAmount);

            var (isSuccess, _, _, _) = await _accountPaymentService.Charge("ReferenceCode", _agent.ToApiCaller(), paymentService);

            _balanceManagementNotificationsServiceMock.Verify(x => x.SendNotificationIfRequired(It.IsAny<AgencyAccount>(), chargingAmount));
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
        private readonly Mock<IBalanceManagementNotificationsService> _balanceManagementNotificationsServiceMock;
        private readonly AccountPaymentService _accountPaymentService;
        private readonly AgentContext _agent =
            new(1, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
                1, string.Empty, true, InAgencyPermissions.All, string.Empty, string.Empty,
                string.Empty, 1, new(), ContractKind.VirtualAccountOrCreditCardPayments);
    }
}
