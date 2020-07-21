using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Payments.Accounts;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Payments;
using HappyTravel.Edo.UnitTests.Utility;
using HappyTravel.Money.Enums;
using Moq;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Payments.Accounts.AccountPaymentServiceTests
{
    public class CanPayWithAccount
    {
        public CanPayWithAccount(Mock<EdoContext> edoContextMock, IDateTimeProvider dateTimeProvider)
        {
            _accountPaymentService = new AccountPaymentService(Mock.Of<IAccountPaymentProcessingService>(), edoContextMock.Object,
                dateTimeProvider, Mock.Of<IAccountManagementService>());

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
                        CreditLimit = 0,
                        IsActive = true
                    },
                    new PaymentAccount
                    {
                        Id = 3,
                        Balance = 5,
                        Currency = Currencies.USD,
                        AgencyId = 3,
                        CreditLimit = 0,
                        IsActive = true
                    },
                    new PaymentAccount
                    {
                        Id = 4,
                        Balance = 0,
                        Currency = Currencies.USD,
                        AgencyId = 4,
                        CreditLimit = 3,
                        IsActive = true
                    }
                }));
        }

        [Fact]
        public async Task Invalid_payment_without_account_should_be_permitted()
        {
            var canPay = await _accountPaymentService.CanPayWithAccount(_invalidAgentContext);
            Assert.False(canPay);
        }

        [Fact]
        public async Task Invalid_cannot_pay_with_account_if_balance_zero()
        {
            var canPay = await _accountPaymentService.CanPayWithAccount(_validAgentContext);
            Assert.False(canPay);
        }

        [Fact]
        public async Task Valid_can_pay_if_balance_greater_zero()
        {
            var canPay = await _accountPaymentService.CanPayWithAccount(_validAgentContextWithPositiveBalance);
            Assert.True(canPay);
        }

        [Fact]
        public async Task Valid_can_pay_if_credit_greater_zero()
        {
            var canPay = await _accountPaymentService.CanPayWithAccount(_validAgentContextWithPositiveCredit);
            Assert.True(canPay);
        }


        private readonly AgentContext _validAgentContext = AgentInfoFactory.CreateByWithCounterpartyAndAgency(1, 1, 1);
        private readonly AgentContext _invalidAgentContext = AgentInfoFactory.CreateByWithCounterpartyAndAgency(2, 2, 2);
        private readonly AgentContext _validAgentContextWithPositiveBalance = AgentInfoFactory.CreateByWithCounterpartyAndAgency(3, 3, 3);
        private readonly AgentContext _validAgentContextWithPositiveCredit = AgentInfoFactory.CreateByWithCounterpartyAndAgency(4, 4, 4);
        private readonly IAccountPaymentService _accountPaymentService;
    }
}
