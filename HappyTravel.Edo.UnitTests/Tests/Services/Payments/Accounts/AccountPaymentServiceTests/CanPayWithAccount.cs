using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Mailing;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
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
        public CanPayWithAccount()
        {
            var edoContextMock = MockEdoContextFactory.Create();

            var dateTimeProvider = new DefaultDateTimeProvider();
            _accountPaymentService = new AccountPaymentService(Mock.Of<IAccountPaymentProcessingService>(), edoContextMock.Object,
                dateTimeProvider, Mock.Of<IBalanceManagementNotificationsService>(),
                Mock.Of<IBookingRecordManager>(), Mock.Of<IBookingDocumentsMailingService>());

            edoContextMock
                .Setup(c => c.AgencyAccounts)
                .Returns(DbSetMockProvider.GetDbSetMock(new List<AgencyAccount>
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


        private readonly AgentContext _validAgentContext = AgentContextFactory.CreateWithAgency(1, 1);
        private readonly AgentContext _invalidAgentContext = AgentContextFactory.CreateWithAgency(2, 2);
        private readonly AgentContext _validAgentContextWithPositiveBalance = AgentContextFactory.CreateWithAgency(3, 3);
        private readonly IAccountPaymentService _accountPaymentService;
    }
}