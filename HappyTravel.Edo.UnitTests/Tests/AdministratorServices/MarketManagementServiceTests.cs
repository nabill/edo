using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Api.AdministratorServices.Locations;
using Api.Models.Locations;
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

            var marketManagementStorageMock = new Mock<IMarketManagementStorage>();
            marketManagementStorageMock.Setup(m => m.Get(It.IsAny<CancellationToken>())).ReturnsAsync(markets);

            _marketManagementService = new MarketManagementService(_edoContextMock.Object, marketManagementStorageMock.Object);

            var strategy = new ExecutionStrategyMock();

            var dbFacade = new Mock<DatabaseFacade>(_edoContextMock.Object);
            dbFacade.Setup(d => d.CreateExecutionStrategy()).Returns(strategy);
            _edoContextMock.Setup(c => c.Database).Returns(dbFacade.Object);
        }


        [Fact]
        public async Task Add_market_should_return_success()
        {
            var languageCode = "en";
            var marketRequest = new MarketRequest(DefualtMarketId, new MultiLanguage<string> { En = "Far East" });

            var (_, isFailure, error) = await _marketManagementService.Add(languageCode, marketRequest, It.IsAny<CancellationToken>());

            Assert.False(isFailure);
        }


        [Fact]
        public async Task Add_market_without_necessary_language_code_should_return_fail()
        {
            var languageCode = "kz";
            var marketRequest = new MarketRequest(DefualtMarketId, new MultiLanguage<string> { Ru = "Дальний восток" });

            var (_, isFailure, error) = await _marketManagementService.Add(languageCode, marketRequest, It.IsAny<CancellationToken>());

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Get_market_should_return_success()
        {
            var markets = await _marketManagementService.Get(It.IsAny<CancellationToken>());

            Assert.Equal(markets.Count, 2);
            Assert.All(markets, m => Assert.NotNull(m.Names.En));
        }


        [Fact]
        public async Task Update_market_should_return_success()
        {
            var languageCode = "en";
            var marketId = 1;
            var marketRequest = new MarketRequest(marketId, new MultiLanguage<string> { En = "Far East" });

            var (_, isFailure, error) = await _marketManagementService.Update(languageCode, marketRequest, It.IsAny<CancellationToken>());

            Assert.False(isFailure);
        }


        [Fact]
        public async Task Update_market_without_necessary_language_code_should_return_fail()
        {
            var languageCode = "kz";
            var marketId = 2;
            var marketRequest = new MarketRequest(marketId, new MultiLanguage<string> { Ru = "Дальний восток" });

            var (_, isFailure, error) = await _marketManagementService.Update(languageCode, marketRequest, It.IsAny<CancellationToken>());

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Update_market_with_wrong_marketId_should_return_fail()
        {
            var languageCode = "ru";
            var marketId = 3;
            var marketRequest = new MarketRequest(marketId, new MultiLanguage<string> { En = "Дальний восток" });

            var (_, isFailure, error) = await _marketManagementService.Update(languageCode, marketRequest, It.IsAny<CancellationToken>());

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Delete_market_should_return_success()
        {
            var marketId = 2;

            var (_, isFailure, error) = await _marketManagementService.Remove(marketId, It.IsAny<CancellationToken>());

            Assert.False(isFailure);
        }


        private void SetupInitialData()
        {
            _edoContextMock
                .Setup(c => c.Markets)
                .Returns(DbSetMockProvider.GetDbSetMock(markets));
        }


        private readonly List<Market> markets = new()
        {
            new Market
            {
                Id = 1,
                Names = new MultiLanguage<string> { En = "Unknown" },
            },
            new Market
            {
                Id = 2,
                Names = new MultiLanguage<string> { En = "Africa" }
            }
        };


        private const int DefualtMarketId = 0;

        private readonly Mock<EdoContext> _edoContextMock;
        private readonly IMarketManagementService _marketManagementService;
    }
}
