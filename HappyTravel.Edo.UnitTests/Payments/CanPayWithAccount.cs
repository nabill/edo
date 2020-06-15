using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Api.Services.Payments;
using HappyTravel.Edo.Api.Services.Payments.Accounts;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Payments;
using HappyTravel.Edo.UnitTests.Infrastructure;
using HappyTravel.Edo.UnitTests.Infrastructure.DbSetMocks;
using HappyTravel.EdoContracts.General.Enums;
using HappyTravel.Money.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Payments
{
    public class CanPayWithAccount
    {
        public CanPayWithAccount(Mock<EdoContext> edoContextMock, IDateTimeProvider dateTimeProvider)
        {
            _accountPaymentService = new AccountPaymentService(Mock.Of<IAdministratorContext>(), Mock.Of<IAccountPaymentProcessingService>(), edoContextMock.Object,
                dateTimeProvider, Mock.Of<IServiceAccountContext>(), Mock.Of<IAgentContext>(), Mock.Of<IPaymentNotificationService>(),
                Mock.Of<IAccountManagementService>(), Mock.Of<ILogger<AccountPaymentService>>());

            edoContextMock
                .Setup(c => c.PaymentAccounts)
                .Returns(DbSetMockProvider.GetDbSetMock(new List<PaymentAccount>
                {
                    new PaymentAccount
                    {
                        Id = 1,
                        Balance = 0,
                        Currency = Currencies.USD,
                        AgencyId = 1,
                        CreditLimit = 0
                    },
                    new PaymentAccount
                    {
                        Id = 3,
                        Balance = 5,
                        Currency = Currencies.USD,
                        AgencyId = 3,
                        CreditLimit = 0
                    },
                    new PaymentAccount
                    {
                        Id = 4,
                        Balance = 0,
                        Currency = Currencies.USD,
                        AgencyId = 4,
                        CreditLimit = 3
                    }
                }));
        }

        [Fact]
        public async Task Invalid_payment_without_account_should_be_permitted()
        {
            var canPay = await _accountPaymentService.CanPayWithAccount(_invalidAgentInfo);
            Assert.False(canPay);
        }

        [Fact]
        public async Task Invalid_cannot_pay_with_account_if_balance_zero()
        {
            var canPay = await _accountPaymentService.CanPayWithAccount(_validAgentInfo);
            Assert.False(canPay);
        }

        [Fact]
        public async Task Valid_can_pay_if_balance_greater_zero()
        {
            var canPay = await _accountPaymentService.CanPayWithAccount(_validAgentInfoWithPositiveBalance);
            Assert.True(canPay);
        }

        [Fact]
        public async Task Valid_can_pay_if_credit_greater_zero()
        {
            var canPay = await _accountPaymentService.CanPayWithAccount(_validAgentInfoWithPositiveCredit);
            Assert.True(canPay);
        }


        private readonly AgentInfo _validAgentInfo = AgentInfoFactory.CreateByWithCounterpartyAndAgency(1, 1, 1);
        private readonly AgentInfo _invalidAgentInfo = AgentInfoFactory.CreateByWithCounterpartyAndAgency(2, 2, 2);
        private readonly AgentInfo _validAgentInfoWithPositiveBalance = AgentInfoFactory.CreateByWithCounterpartyAndAgency(3, 3, 3);
        private readonly AgentInfo _validAgentInfoWithPositiveCredit = AgentInfoFactory.CreateByWithCounterpartyAndAgency(4, 4, 4);
        private readonly IAccountPaymentService _accountPaymentService;
    }
}
