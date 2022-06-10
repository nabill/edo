using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Mailing;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Payments.Accounts;
using HappyTravel.Edo.Data.Payments;
using HappyTravel.Edo.UnitTests.Utility;
using HappyTravel.Money.Enums;
using Moq;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Payments.Accounts.AccountPaymentServiceTests
{
    public class CanPayWithAccount
    {
        [Fact]
        public async Task Invalid_payment_without_account_should_be_permitted()
        {
            var accountPaymentService = GetPaymentService(_invalidAgentContext);
            var canPay = await accountPaymentService.CanPayWithAccount();

            Assert.False(canPay);
        }


        [Fact]
        public async Task Invalid_cannot_pay_with_account_if_balance_zero()
        {
            var accountPaymentService = GetPaymentService(_validAgentContext);
            var canPay = await accountPaymentService.CanPayWithAccount();

            Assert.False(canPay);
        }


        [Fact]
        public async Task Valid_can_pay_if_balance_greater_zero()
        {
            var accountPaymentService = GetPaymentService(_validAgentContextWithPositiveBalance);
            var canPay = await accountPaymentService.CanPayWithAccount();

            Assert.True(canPay);
        }
        

        private static IAccountPaymentService GetPaymentService(AgentContext agentContext)
        {
            var edoContextMock = MockEdoContextFactory.Create();
            var dateTimeProvider = new DefaultDateTimeProvider();
            var agentContextServiceMock = new Mock<IAgentContextService>();
            
            edoContextMock
                .Setup(c => c.AgencyAccounts)
                .Returns(DbSetMockProvider.GetDbSetMock(Accounts));

            agentContextServiceMock
                .Setup(x => x.GetAgent())
                .ReturnsAsync(agentContext);
            
            return new AccountPaymentService(Mock.Of<IAccountPaymentProcessingService>(), edoContextMock.Object,
                dateTimeProvider, Mock.Of<IBalanceManagementNotificationsService>(),
                Mock.Of<IBookingRecordManager>(), Mock.Of<IBookingDocumentsMailingService>(),
                agentContextServiceMock.Object);
        }


        private static readonly List<AgencyAccount> Accounts = new()
        {
            new AgencyAccount
            {
                Id = 1,
                Balance = 0,
                Currency = Currencies.USD,
                AgencyId = 1,
                IsActive = true
            },
            new AgencyAccount
            {
                Id = 3,
                Balance = 5,
                Currency = Currencies.USD,
                AgencyId = 3,
                IsActive = true
            }
        };


        private readonly AgentContext _validAgentContext = AgentContextFactory.CreateWithAgency(1, 1);
        private readonly AgentContext _invalidAgentContext = AgentContextFactory.CreateWithAgency(2, 2);
        private readonly AgentContext _validAgentContextWithPositiveBalance = AgentContextFactory.CreateWithAgency(3, 3);
    }
}