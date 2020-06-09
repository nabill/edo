using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Payments.Accounts;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Edo.Data.Payments;
using HappyTravel.Edo.UnitTests.Infrastructure.DbSetMocks;
using HappyTravel.Money.Enums;
using HappyTravel.Money.Models;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Moq;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Payments
{
    public class CounteprartyAccountsTests
    {
        public CounteprartyAccountsTests(Mock<EdoContext> edoContextMock,
            IDateTimeProvider dateTimeProvider)
        {
            var entityLockerMock = new Mock<IEntityLocker>();

            entityLockerMock.Setup(l => l.Acquire<It.IsAnyType>(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(Result.Ok()));

            _mockedEdoContext = edoContextMock.Object;

            _counterpartyAccountService = new CounterpartyAccountService(_mockedEdoContext, entityLockerMock.Object, Mock.Of<IAccountBalanceAuditService>());

            var stradegy = new ExecutionStrategyMock();

            var dbFacade = new Mock<DatabaseFacade>(_mockedEdoContext);
            dbFacade.Setup(d => d.CreateExecutionStrategy()).Returns(stradegy);
            edoContextMock.Setup(c => c.Database).Returns(dbFacade.Object);

            edoContextMock
                .Setup(c => c.Counterparties)
                .Returns(DbSetMockProvider.GetDbSetMock(new List<Counterparty>
                {
                    new Counterparty
                    {
                        Id = 1,
                        Name = "WothoutAgency"
                    },
                    new Counterparty
                    {
                        Id = 2,
                        Name = "WithBadAgency"
                    },
                    new Counterparty
                    {
                        Id = 3,
                        Name = "WithGoodAgency"
                    },
                    new Counterparty
                    {
                        Id = 4,
                        Name = "ForDecrease"
                    },
                    new Counterparty
                    {
                        Id = 5,
                        Name = "ForIncrease"
                    },
                    new Counterparty
                    {
                        Id = 6,
                        Name = "WithDifferentCurrencies"
                    },
                    new Counterparty
                    {
                        Id = 7,
                        Name = "ForNegativeChecks"
                    },
                }));

            edoContextMock
                .Setup(c => c.Agencies)
                .Returns(DbSetMockProvider.GetDbSetMock(new List<Agency>
                {
                    new Agency
                    {
                        Id = 2,
                        CounterpartyId = 2,
                        Name = "BadAgencyNoAccount",
                        IsDefault = true
                    },
                    new Agency
                    {
                        Id = 3,
                        CounterpartyId = 3,
                        Name = "GoodAgencyWithAccount",
                        IsDefault = true
                    },
                    new Agency
                    {
                        Id = 6,
                        CounterpartyId = 6,
                        Name = "WithDifferentCurrancy",
                        IsDefault = true
                    },
                    new Agency
                    {
                        Id = 7,
                        CounterpartyId = 7,
                        Name = "ForNegativeChecks",
                        IsDefault = true
                    },
                }));

            edoContextMock
                .Setup(c => c.PaymentAccounts)
                .Returns(DbSetMockProvider.GetDbSetMock(new List<PaymentAccount>
                {
                    new PaymentAccount
                    {
                        Id = 3,
                        Balance = 0,
                        Currency = Currencies.USD,
                        AgencyId = 3,
                        CreditLimit = 0
                    },
                    new PaymentAccount
                    {
                        Id = 6,
                        Balance = 0,
                        Currency = Currencies.EUR,
                        AgencyId = 6,
                        CreditLimit = 0
                    },
                    new PaymentAccount
                    {
                        Id = 7,
                        Balance = 0,
                        Currency = Currencies.USD,
                        AgencyId = 7,
                        CreditLimit = 0
                    }
                }));

            edoContextMock
                .Setup(c => c.CounterpartyAccounts)
                .Returns(DbSetMockProvider.GetDbSetMock(new List<CounterpartyAccount>
                {
                    new CounterpartyAccount
                    {
                        Id = 1,
                        Balance = 1000,
                        Currency = Currencies.USD,
                        CounterpartyId = 1
                    },
                    new CounterpartyAccount
                    {
                        Id = 2,
                        Balance = 1000,
                        Currency = Currencies.USD,
                        CounterpartyId = 2
                    },
                    new CounterpartyAccount
                    {
                        Id = 3,
                        Balance = 1000,
                        Currency = Currencies.USD,
                        CounterpartyId = 3
                    },
                    new CounterpartyAccount
                    {
                        Id = 4,
                        Balance = 1000,
                        Currency = Currencies.USD,
                        CounterpartyId = 4
                    },
                    new CounterpartyAccount
                    {
                        Id = 5,
                        Balance = 1000,
                        Currency = Currencies.USD,
                        CounterpartyId = 5
                    },
                    new CounterpartyAccount
                    {
                        Id = 6,
                        Balance = 1000,
                        Currency = Currencies.USD,
                        CounterpartyId = 6
                    },
                    new CounterpartyAccount
                    {
                        Id = 7,
                        Balance = 1000,
                        Currency = Currencies.USD,
                        CounterpartyId = 7
                    },
                }));
        }

        [Fact]
        public async Task Existing_currency_balance_should_be_shown()
        {
            var (isSuccess, _, balanceInfo) = await _counterpartyAccountService.GetBalance(3, Currencies.USD);
            Assert.True(isSuccess);
            Assert.Equal(1000, balanceInfo.Balance);
        }

        [Fact]
        public async Task Not_existing_currency_balance_show_should_fail()
        {
            var (_, isFailure) = await _counterpartyAccountService.GetBalance(3, Currencies.EUR);
            Assert.True(isFailure);
        }

        [Fact]
        public async Task Add_money_with_currency_mismatch_should_fail()
        {
            var (_, isFailure) = await _counterpartyAccountService.AddMoney(
                3, new PaymentData(1, Currencies.EUR, "kek"), _user);
            Assert.True(isFailure);
        }

        [Fact]
        public async Task Add_money_to_unexistent_account_should_fail()
        {
            var (_, isFailure) = await _counterpartyAccountService.AddMoney(
                0, new PaymentData(1, Currencies.USD, "kek"), _user);
            Assert.True(isFailure);
        }

        [Fact]
        public async Task Add_money_with_negative_amount_should_fail()
        {
            var (_, isFailure) = await _counterpartyAccountService.AddMoney(
                7, new PaymentData(-1, Currencies.USD, "kek"), _user);
            Assert.True(isFailure);
        }

        [Fact]
        public async Task Add_money_to_suitable_account_should_increase_balance()
        {
            var affectedAccount = _mockedEdoContext.CounterpartyAccounts.Single(a => a.Id == 5);

            var (isSuccess, _) = await _counterpartyAccountService.AddMoney(
                5, new PaymentData(1, Currencies.USD, "kek"), _user);

            Assert.True(isSuccess);
            Assert.Equal(1001, affectedAccount.Balance);
        }

        [Fact]
        public async Task Subtract_money_with_currency_mismatch_should_fail()
        {
            var (_, isFailure) = await _counterpartyAccountService.SubtractMoney(
                3, new PaymentCancellationData(1, Currencies.EUR), _user);
            Assert.True(isFailure);
        }

        [Fact]
        public async Task Subtract_money_with_negative_amount_should_fail()
        {
            var (_, isFailure) = await _counterpartyAccountService.SubtractMoney(
                7, new PaymentCancellationData(-1, Currencies.USD), _user);
            Assert.True(isFailure);
        }

        [Fact]
        public async Task Subtract_money_from_suitable_account_should_decrease_balance()
        {
            var affectedAccount = _mockedEdoContext.CounterpartyAccounts.Single(a => a.Id == 4);

            var (isSuccess, _) = await _counterpartyAccountService.SubtractMoney(
                4, new PaymentCancellationData(1, Currencies.USD), _user);

            Assert.True(isSuccess);
            Assert.Equal(999, affectedAccount.Balance);
        }

        [Fact]
        public async Task Transfer_to_default_agency_currency_mismatch_should_fail()
        {
            var (_, isFailure) = await _counterpartyAccountService.TransferToDefaultAgency(
                3, new MoneyAmount(1, Currencies.EUR), _user);

            Assert.True(isFailure);
        }

        [Fact]
        public async Task Transfer_to_default_agency_which_not_exists_should_fail()
        {
            var (_, isFailure) = await _counterpartyAccountService.TransferToDefaultAgency(
                1, new MoneyAmount(1, Currencies.USD), _user);

            Assert.True(isFailure);
        }

        [Fact]
        public async Task Transfer_to_default_agency_without_account_should_fail()
        {
            var (_, isFailure) = await _counterpartyAccountService.TransferToDefaultAgency(
                2, new MoneyAmount(1, Currencies.USD), _user);

            Assert.True(isFailure);
        }

        [Fact]
        public async Task Transfer_to_default_agency_with_different_currency_should_fail()
        {
            var (_, isFailure) = await _counterpartyAccountService.TransferToDefaultAgency(
                6, new MoneyAmount(1, Currencies.USD), _user);

            Assert.True(isFailure);
        }

        [Fact]
        public async Task Transfer_to_default_agency_with_negative_amount_should_fail()
        {
            var (_, isFailure) = await _counterpartyAccountService.TransferToDefaultAgency(
                7, new MoneyAmount(-1, Currencies.USD), _user);

            Assert.True(isFailure);
        }

        [Fact]
        public async Task Transfer_to_default_agency_should_affect_the_accounts()
        {
            var counterpartyAccount = _mockedEdoContext.CounterpartyAccounts.Single(a => a.Id == 3);
            var paymentAccount = _mockedEdoContext.PaymentAccounts.Single(a => a.Id == 3);

            var (isSuccess, _) = await _counterpartyAccountService.TransferToDefaultAgency(
                3, new MoneyAmount(1, Currencies.USD), _user);

            Assert.True(isSuccess);
            Assert.Equal(999, counterpartyAccount.Balance);
            Assert.Equal(1, paymentAccount.Balance);
        }


        private readonly EdoContext _mockedEdoContext;
        private readonly UserInfo _user = new UserInfo(1, UserTypes.Admin);
        private readonly ICounterpartyAccountService _counterpartyAccountService;
    }
}
