using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Api.AdministratorServices.Locations;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Locations;
using HappyTravel.Edo.UnitTests.Mocks;
using HappyTravel.Edo.UnitTests.Utility;
using HappyTravel.MultiLanguage;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Moq;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.AdministratorServices
{
    public class MarketManagementServiceTests
    {
        public MarketManagementServiceTests()
        {
            _edoContextMock = MockEdoContextFactory.Create();
            SetupInitialData();

            var fakeDoubleFlow = new FakeDoubleFlow();
            _marketManagementService = new MarketManagementService(_edoContextMock.Object, fakeDoubleFlow);

            var strategy = new ExecutionStrategyMock();

            var dbFacade = new Mock<DatabaseFacade>(_edoContextMock.Object);
            dbFacade.Setup(d => d.CreateExecutionStrategy()).Returns(strategy);
            _edoContextMock.Setup(c => c.Database).Returns(dbFacade.Object);
        }


        [Fact]
        public async Task Add_market_should_return_success()
        {
            var languageCode = "en";
            var namesRequest = new MultiLanguage<string> { En = "Far East" };

            var (_, isFailure, error) = await _marketManagementService.AddMarket(languageCode, namesRequest, It.IsAny<CancellationToken>());

            Assert.False(isFailure);
        }


        [Fact]
        public async Task Add_market_without_necessary_language_code_should_return_fail()
        {
            var languageCode = "kz";
            var namesRequest = new MultiLanguage<string> { Ru = "Дальний восток" };

            var (_, isFailure, error) = await _marketManagementService.AddMarket(languageCode, namesRequest, It.IsAny<CancellationToken>());

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Get_market_should_return_success()
        {
            var languageCode = "en";

            var markets = await _marketManagementService.GetMarkets(languageCode, It.IsAny<CancellationToken>());

            Assert.Equal(markets.Count, 2);
        }


        [Fact]
        public async Task Update_market_should_return_success()
        {
            var languageCode = "en";
            var namesRequest = new MultiLanguage<string> { En = "Far East" };

            var (_, isFailure, error) = await _marketManagementService.ModifyMarket(languageCode, 1, namesRequest, It.IsAny<CancellationToken>());

            Assert.False(isFailure);
        }


        [Fact]
        public async Task Update_market_without_necessary_language_code_should_return_fail()
        {
            var languageCode = "kz";
            var marketId = 2;
            var namesRequest = new MultiLanguage<string> { Ru = "Дальний восток" };

            var (_, isFailure, error) = await _marketManagementService.ModifyMarket(languageCode, marketId, namesRequest, It.IsAny<CancellationToken>());

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Update_market_with_wrong_marketId_should_return_fail()
        {
            var languageCode = "ru";
            var marketId = 3;
            var namesRequest = new MultiLanguage<string> { En = "Дальний восток" };

            var (_, isFailure, error) = await _marketManagementService.ModifyMarket(languageCode, marketId, namesRequest, It.IsAny<CancellationToken>());

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Delete_market_should_return_success()
        {
            var marketId = 2;

            var (_, isFailure, error) = await _marketManagementService.RemoveMarket(marketId, It.IsAny<CancellationToken>());

            Assert.False(isFailure);
        }


        private void SetupInitialData()
        {
            _edoContextMock
                .Setup(c => c.Markets)
                .Returns(DbSetMockProvider.GetDbSetMock(new List<Market>
                {
                    new Market
                    {
                        Id = 1,
                        Names = JsonDocument.Parse("{\"en\": \"Unknown\"}"),
                    },
                    new Market
                    {
                        Id = 2,
                        Names = JsonDocument.Parse("{\"en\": \"Africa\"}"),
                    }
                }));
        }

        private readonly Mock<EdoContext> _edoContextMock;
        private readonly IMarketManagementService _marketManagementService;
    }
}
