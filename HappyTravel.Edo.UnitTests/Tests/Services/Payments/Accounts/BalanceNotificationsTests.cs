using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.DataFormatters;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Mailing;
using HappyTravel.Edo.Api.NotificationCenter.Services;
using HappyTravel.Edo.Api.Services.Payments.Accounts;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Edo.Data.Payments;
using HappyTravel.Edo.Notifications.Enums;
using HappyTravel.Edo.UnitTests.Mocks;
using HappyTravel.Edo.UnitTests.Utility;
using HappyTravel.Money.Enums;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Payments.Accounts
{
    public class BalanceNotificationsTests
    {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        public BalanceNotificationsTests()
        {
            var entityLockerMock = new Mock<IEntityLocker>();
            entityLockerMock.Setup(l => l.Acquire<It.IsAnyType>(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(Result.Success()));

            var edoContextMock = MockEdoContextFactory.Create();
            _mockedEdoContext = edoContextMock.Object;

            var strategy = new ExecutionStrategyMock();
            var dbFacade = new Mock<DatabaseFacade>(_mockedEdoContext);
            dbFacade.Setup(d => d.CreateExecutionStrategy()).Returns(strategy);
            edoContextMock.Setup(c => c.Database).Returns(dbFacade.Object);

            edoContextMock
                .Setup(c => c.BalanceNotificationSettings)
                .Returns(DbSetMockProvider.GetDbSetMock(new List<BalanceNotificationSetting>
                {
                    new BalanceNotificationSetting
                    {
                        AgencyAccountId = 1,
                        Thresholds = new []{100, 200, 500}
                    },
                    new BalanceNotificationSetting
                    {
                        AgencyAccountId = 2,
                        Thresholds = new int[0]
                    }
                }));


            edoContextMock
                .Setup(c => c.Agencies)
                .Returns(DbSetMockProvider.GetDbSetMock(new List<Agency>
                {
                    new Agency
                    {
                        Id = 1,
                        Name = "AgencyName1"
                    },
                    new Agency
                    {
                        Id = 2,
                        Name = "AgencyName2"
                    }
                }));


            edoContextMock
                .Setup(c => c.AgencyAccounts)
                .Returns(DbSetMockProvider.GetDbSetMock(new List<AgencyAccount>
                {
                    new AgencyAccount
                    {
                        Id = 1,
                        AgencyId = 1,
                        Currency = Currencies.USD
                    },
                    new AgencyAccount
                    {
                        Id = 2,
                        AgencyId = 2,
                        Currency = Currencies.AED
                    }
                }));
        }


        [Fact]
        public async Task Crossing_threshold_should_call_notification()
        {
            InitializeMock();
            var service = CreateService();

            await service.SendNotificationIfRequired(1, 10000, 0);

            _notificationServiceMock.Verify(
                x => x.Send(It.IsAny<DataWithCompanyInfo>(), It.IsAny<NotificationTypes>(), It.IsAny<string>(), It.IsAny<string>()));
        }


        [Theory]
        [InlineData(10000, 9000)]
        [InlineData(400, 300)]
        [InlineData(1000, 500)]
        [InlineData(99, 50)]
        public async Task Not_crossing_any_threshold_should_not_send_notifications(decimal initialAmount, decimal resultingAmount)
        {
            InitializeMock();
            var service = CreateService();

            await service.SendNotificationIfRequired(1, initialAmount, resultingAmount);

            _notificationServiceMock.Verify(
                x => x.Send(It.IsAny<DataWithCompanyInfo>(), It.IsAny<NotificationTypes>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }


        [Theory]
        [InlineData(10000, 0, 100)]
        [InlineData(10000, 150, 200)]
        [InlineData(150, 0, 100)]
        [InlineData(100, 0, 100)]
        [InlineData(500, 499, 500)]
        public async Task Should_use_lowest_threshold(decimal initialAmount, decimal resultingAmount, int expectedThreshold)
        {
            InitializeMock();
            var service = CreateService();
            AccountBalanceManagementNotificationData actualMailData = null;
            SaveMailData();

            await service.SendNotificationIfRequired(1, initialAmount, resultingAmount);

            Assert.Equal(expectedThreshold, actualMailData.Threshold);

            void SaveMailData()
                => _notificationServiceMock
                    .Setup(x => x.Send(It.IsAny<DataWithCompanyInfo>(), It.IsAny<NotificationTypes>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Callback<DataWithCompanyInfo, NotificationTypes, string, string>((data, _, _, _)
                        => actualMailData = (AccountBalanceManagementNotificationData)data);
        }


        [Fact]
        public async Task Should_send_correct_data()
        {
            InitializeMock();
            var service = CreateService();
            AccountBalanceManagementNotificationData actualMailData = null;
            SaveMailData();

            await service.SendNotificationIfRequired(1, 10000, 57);

            Assert.Equal(1, actualMailData.AgencyAccountId);
            Assert.Equal(1, actualMailData.AgencyId);
            Assert.Equal("AgencyName1", actualMailData.AgencyName);
            Assert.Equal(EnumFormatters.FromDescription(Currencies.USD), actualMailData.Currency);
            Assert.Equal(MoneyFormatter.ToCurrencyString(57, Currencies.USD), actualMailData.NewAmount);

            void SaveMailData()
                => _notificationServiceMock
                    .Setup(x => x.Send(It.IsAny<DataWithCompanyInfo>(), It.IsAny<NotificationTypes>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Callback<DataWithCompanyInfo, NotificationTypes, string, string>((data, _, _, _)
                        => actualMailData = (AccountBalanceManagementNotificationData) data);
        }


        private BalanceManagementNotificationsService CreateService()
            => new BalanceManagementNotificationsService(_mockedEdoContext,
                _notificationServiceMock.Object,
                Options.Create(new BalanceManagementNotificationsOptions
                {
                    AccountsEmail = "accounts@email.com",
                    BalanceManagementNotificationTemplateId = "templateId"
                }));


        private void InitializeMock()
            => _notificationServiceMock = new Mock<INotificationService>();


        private readonly EdoContext _mockedEdoContext;
        private Mock<INotificationService> _notificationServiceMock;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
    }
}
