using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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

            _marketManagementStorageMock = new Mock<IMarketManagementStorage>();
            _marketManagementStorageMock.Setup(m => m.Get(It.IsAny<CancellationToken>())).ReturnsAsync(markets);

            _marketManagementService = new MarketManagementService(_edoContextMock.Object, _marketManagementStorageMock.Object);

            var strategy = new ExecutionStrategyMock();

            var dbFacade = new Mock<DatabaseFacade>(_edoContextMock.Object);
            dbFacade.Setup(d => d.CreateExecutionStrategy()).Returns(strategy);
            _edoContextMock.Setup(c => c.Database).Returns(dbFacade.Object);
        }


        [Fact]
        public async Task Add_market_should_return_success()
        {
            const string languageCode = "en";
            var marketRequest = new MarketRequest(DefualtMarketId, "Far East");

            var (_, isFailure, error) = await _marketManagementService.Add(languageCode, marketRequest, It.IsAny<CancellationToken>());

            Assert.False(isFailure);
        }

        // Commented until we will be back to multilanguage model
        // [Fact]
        // public async Task Add_market_without_necessary_language_code_should_return_fail()
        // {
        //     const string languageCode = "kz";
        //     var marketRequest = new MarketRequest(DefualtMarketId, "Дальний восток");

        //     var (_, isFailure, error) = await _marketManagementService.Add(languageCode, marketRequest, It.IsAny<CancellationToken>());

        //     Assert.True(isFailure);
        // }


        [Fact]
        public async Task Get_market_should_return_success()
        {
            var marketsResponse = await _marketManagementService.Get(It.IsAny<CancellationToken>());

            Assert.Equal(marketsResponse.Count, markets.Count);
            Assert.All(marketsResponse, m => Assert.NotNull(m.Name));
        }


        [Fact]
        public async Task Update_market_should_return_success()
        {
            const string languageCode = "en";
            const int marketId = 1;
            var marketRequest = new MarketRequest(marketId, "Far East");

            var (_, isFailure, error) = await _marketManagementService.Update(languageCode, marketRequest, It.IsAny<CancellationToken>());

            Assert.False(isFailure);
        }


        // Commented until we will be back to multilanguage model
        // [Fact]
        // public async Task Update_market_without_necessary_language_code_should_return_fail()
        // {
        //     const string languageCode = "kz";
        //     const int marketId = 2;
        //     var marketRequest = new MarketRequest(marketId, "Дальний восток");

        //     var (_, isFailure, error) = await _marketManagementService.Update(languageCode, marketRequest, It.IsAny<CancellationToken>());

        //     Assert.True(isFailure);
        // }


        [Fact]
        public async Task Update_market_with_wrong_marketId_should_return_fail()
        {
            const string languageCode = "ru";
            const int marketId = 4;
            var marketRequest = new MarketRequest(marketId, "Дальний восток");

            var (_, isFailure, error) = await _marketManagementService.Update(languageCode, marketRequest, It.IsAny<CancellationToken>());

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Delete_market_should_return_success()
        {
            const int marketId = 2;

            var (_, isFailure, error) = await _marketManagementService.Remove(marketId, It.IsAny<CancellationToken>());

            Assert.False(isFailure);
        }


        [Fact]
        public async Task Delete_unknown_market_should_return_fail()
        {
            const int marketId = 1;

            var (_, isFailure, error) = await _marketManagementService.Remove(marketId, It.IsAny<CancellationToken>());

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Get_countries_should_return_success()
        {
            const int marketId = 2;
            var countriesById = countries.Where(c => c.MarketId == marketId).ToList();
            _marketManagementStorageMock
                .Setup(m => m.GetMarketCountries(marketId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(countriesById);
            var marketManagementService = new MarketManagementService(_edoContextMock.Object, _marketManagementStorageMock.Object);


            var (_, isFailure, countriesResponse) = await marketManagementService.GetMarketCountries(marketId, It.IsAny<CancellationToken>());

            Assert.Equal(countriesResponse.Count, countriesById.Count);
            Assert.All(countriesResponse, m => Assert.NotNull(m.Names.GetValueOrDefault("en")));
        }


        [Fact]
        public async Task Get_countries_with_wrong_marketId_should_return_fail()
        {
            const int marketId = 4;
            var countriesById = countries.Where(c => c.MarketId == marketId).ToList();
            _marketManagementStorageMock
                .Setup(m => m.GetMarketCountries(marketId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(countriesById);
            var marketManagementService = new MarketManagementService(_edoContextMock.Object, _marketManagementStorageMock.Object);


            var (_, isFailure, response, error) = await marketManagementService.GetMarketCountries(marketId, It.IsAny<CancellationToken>());

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Add_countries_to_market_should_return_success()
        {
            const int marketId = 2;
            var listCodes = new List<string> { "GB", "US" };
            var countriesRequest = new CountryRequest(marketId, listCodes);

            var (_, isFailure, error) = await _marketManagementService.UpdateMarketCountries(countriesRequest, It.IsAny<CancellationToken>());

            Assert.False(isFailure);
        }


        [Fact]
        public async Task Add_countries_with_wrong_marketId_should_return_fail()
        {
            const int marketId = 4;
            var listCodes = new List<string> { "GB", "US" };
            var countriesRequest = new CountryRequest(marketId, listCodes);

            var (_, isFailure, error) = await _marketManagementService.UpdateMarketCountries(countriesRequest, It.IsAny<CancellationToken>());

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Add_countries_to_market_with_wrong_country_code_should_return_fail()
        {
            const int marketId = 2;
            var listCodes = new List<string> { "GB", "US", "AL" };
            var countriesRequest = new CountryRequest(marketId, listCodes);

            var (_, isFailure, error) = await _marketManagementService.UpdateMarketCountries(countriesRequest, It.IsAny<CancellationToken>());

            Assert.True(isFailure);
        }


        private void SetupInitialData()
        {
            _edoContextMock
                .Setup(c => c.Markets)
                .Returns(DbSetMockProvider.GetDbSetMock(markets));

            _edoContextMock
                .Setup(c => c.Countries)
                .Returns(DbSetMockProvider.GetDbSetMock(countries));
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
                Names = new MultiLanguage<string> { En = "CIS" }
            },
            new Market
            {
                Id = 3,
                Names = new MultiLanguage<string> { En = "Africa"}
            }
        };


        private readonly List<Country> countries = new()
        {
            new Country
            {
                Code = "RU",
                Names = new MultiLanguage<string> { En = "Russia" },
                MarketId = 2
            },
            new Country
            {
                Code = "KZ",
                Names = new MultiLanguage<string> { En = "Kazakhstan" },
                MarketId = 2
            },
            new Country
            {
                Code = "KR",
                Names = new MultiLanguage<string> { En = "Kyrgyzstan" },
                MarketId = 2
            },
            new Country
            {
                Code = "GB",
                Names = new MultiLanguage<string> { En = "Great Britan" },
                MarketId = 1
            },
            new Country
            {
                Code = "US",
                Names = new MultiLanguage<string> { En = "United States" },
                MarketId = 1
            },
            new Country
            {
                Code = "AL",
                Names = new MultiLanguage<string> { En = "Albania" },
                MarketId = 3
            },
        };


        private const int DefualtMarketId = 0;

        private readonly Mock<EdoContext> _edoContextMock;
        private readonly Mock<IMarketManagementStorage> _marketManagementStorageMock;
        private readonly IMarketManagementService _marketManagementService;
    }
}
