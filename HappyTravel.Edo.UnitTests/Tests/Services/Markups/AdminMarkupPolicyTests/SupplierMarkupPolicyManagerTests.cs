using System.Threading;
using System.Collections.Generic;
using Api.Services.Markups;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Data.Locations;
using HappyTravel.Edo.Api.Services.Markups;
using HappyTravel.Edo.Api.Services.Messaging;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Edo.Data.Markup;
using HappyTravel.Edo.UnitTests.Mocks;
using HappyTravel.Edo.UnitTests.Utility;
using HappyTravel.Money.Enums;
using HappyTravel.MultiLanguage;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Moq;
using Xunit;
using System.Threading.Tasks;
using Api.Models.Markups.Supplier;
using CSharpFunctionalExtensions;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Markups.AdminMarkupPolicyTests
{
    public class SupplierMarkupPolicyManagerTests
    {
        public SupplierMarkupPolicyManagerTests()
        {
            _edoContextMock = MockEdoContextFactory.Create();
            SetupInitialData();

            _supplierMarkupPolicyManager = new SupplierMarkupPolicyManager(_edoContextMock.Object,
                Mock.Of<IDateTimeProvider>(), Mock.Of<IDisplayedMarkupFormulaService>(),
                Mock.Of<IMessageBus>());

            var strategy = new ExecutionStrategyMock();

            var dbFacade = new Mock<DatabaseFacade>(_edoContextMock.Object);
            dbFacade.Setup(d => d.CreateExecutionStrategy()).Returns(strategy);
            _edoContextMock.Setup(c => c.Database).Returns(dbFacade.Object);
        }


        [Fact]
        public async Task Add_supplier_country_markup_should_return_success()
        {
            var request = new SupplierMarkupRequest("Description", 10, "KZ",
                DestinationMarkupScopeTypes.Country, "jumeirah");

            var (_, isFailure, error) = await _supplierMarkupPolicyManager.Add(request, It.IsAny<CancellationToken>());

            Assert.False(isFailure);
        }


        [Fact]
        public async Task Add_supplier_country_markup_without_supplier_should_return_fail()
        {
            var request = new SupplierMarkupRequest("Description", 10, "KZ",
                DestinationMarkupScopeTypes.Country, string.Empty);

            var (_, isFailure, error) = await _supplierMarkupPolicyManager.Add(request, It.IsAny<CancellationToken>());

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Add_supplier_country_markup_with_wrong_value_should_return_fail()
        {
            var request = new SupplierMarkupRequest("Description", 0, "KZ",
                DestinationMarkupScopeTypes.Country, "jumeirah");

            var (_, isFailure, error) = await _supplierMarkupPolicyManager.Add(request, It.IsAny<CancellationToken>());

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Add_supplier_country_markup_with_wrong_country_should_return_fail()
        {
            var request = new SupplierMarkupRequest("Description", 10, "GB",
                DestinationMarkupScopeTypes.Country, "jumeirah");

            var (_, isFailure, error) = await _supplierMarkupPolicyManager.Add(request, It.IsAny<CancellationToken>());

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Add_supplier_country_markup_already_exists_should_return_fail()
        {
            var request = new SupplierMarkupRequest("Description", 10, "RU",
                DestinationMarkupScopeTypes.Country, "jumeirah");

            var (_, isFailure, error) = await _supplierMarkupPolicyManager.Add(request, It.IsAny<CancellationToken>());

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Add_supplier_country_markup_with_unexpected_type_should_return_fail()
        {
            var request = new SupplierMarkupRequest("Description", 10, "2",
                DestinationMarkupScopeTypes.Market, "jumeirah");

            var (_, isFailure, error) = await _supplierMarkupPolicyManager.Add(request, It.IsAny<CancellationToken>());

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Modify_supplier_country_markup_with_wrong_policyId_should_return_fail()
        {
            var request = new SupplierMarkupRequest("Description", 10, "2",
                 DestinationMarkupScopeTypes.Market, "jumeirah");

            var (_, isFailure, error) = await _supplierMarkupPolicyManager.Modify(7, request, It.IsAny<CancellationToken>());

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Modify_supplier_country_markup_with_unupdated_data_should_return_fail()
        {
            var request = new SupplierMarkupRequest("Description", 10, "2",
                 DestinationMarkupScopeTypes.Market, "jumeirah");

            var (_, isFailure, error) = await _supplierMarkupPolicyManager.Modify(2, request, It.IsAny<CancellationToken>());

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Modify_supplier_country_markup_with_wrong_value_should_return_fail()
        {
            var request = new SupplierMarkupRequest("Description", 0, "RU",
                 DestinationMarkupScopeTypes.Country, "jumeirah");

            var (_, isFailure, error) = await _supplierMarkupPolicyManager.Modify(2, request, It.IsAny<CancellationToken>());

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Modify_supplier_country_markup_with_not_supplier_policyId_should_return_fail()
        {
            var request = new SupplierMarkupRequest("Description", 10, "1",
                 DestinationMarkupScopeTypes.Market, "jumeirah");

            var (_, isFailure, error) = await _supplierMarkupPolicyManager.Modify(1, request, It.IsAny<CancellationToken>());

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Modify_supplier_country_markup_with_not_supplier_code_should_return_fail()
        {
            var request = new SupplierMarkupRequest("Description", 10, "RU",
                 DestinationMarkupScopeTypes.Country, "jumeirah");

            var (_, isFailure, error) = await _supplierMarkupPolicyManager.Modify(3, request, It.IsAny<CancellationToken>());

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

            _edoContextMock
                .Setup(c => c.Agencies)
                .Returns(DbSetMockProvider.GetDbSetMock(agencies));

            _edoContextMock
                .Setup(c => c.MarkupPolicies)
                .Returns(DbSetMockProvider.GetDbSetMock(markupPolicies));
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
            },
            new Market
            {
                Id = 4,
                Names = new MultiLanguage<string> { En = "Europe"}
            }
        };


        private readonly List<Country> countries = new()
        {
            new Country
            {
                Code = "RU"
            },
            new Country
            {
                Code = "KZ"
            }
        };


        private readonly List<Agency> agencies = new()
        {
            new Agency
            {
                Id = 1,
                Name = "Test 1"
            },
            new Agency
            {
                Id = 2,
                Name = "Test 2"
            },
            new Agency
            {
                Id = 3,
                Name = "Test 3"
            }
        };


        private readonly List<MarkupPolicy> markupPolicies = new()
        {
            new MarkupPolicy
            {
                Id = 1,
                Value = 2,
                Currency = Currencies.USD,
                FunctionType = MarkupFunctionType.Percent,
                Description = "Markup 1",
                SubjectScopeId = "1",
                SubjectScopeType = SubjectMarkupScopeTypes.Agency,
                DestinationScopeId = "1",
                DestinationScopeType = DestinationMarkupScopeTypes.Market,
                SupplierCode = "jumeirah"
            },
            new MarkupPolicy
            {
                Id = 2,
                Value = 5,
                Currency = Currencies.USD,
                FunctionType = MarkupFunctionType.Percent,
                Description = "Markup 2",
                SubjectScopeId = string.Empty,
                SubjectScopeType = SubjectMarkupScopeTypes.Global,
                DestinationScopeId = "RU",
                DestinationScopeType = DestinationMarkupScopeTypes.Country,
                SupplierCode = "jumeirah"
            },
            new MarkupPolicy
            {
                Id = 3,
                Value = -11,
                Currency = Currencies.USD,
                FunctionType = MarkupFunctionType.Percent,
                Description = "Markup 3",
                SubjectScopeId = "1",
                SubjectScopeType = SubjectMarkupScopeTypes.Agency,
                DestinationScopeId = "RU",
                DestinationScopeType = DestinationMarkupScopeTypes.Country,
                SupplierCode = null
            },
            new MarkupPolicy
            {
                Id = 4,
                Value = 1,
                Currency = Currencies.USD,
                FunctionType = MarkupFunctionType.Percent,
                Description = "Markup 4",
                SubjectScopeId = string.Empty,
                SubjectScopeType = SubjectMarkupScopeTypes.Global,
                DestinationScopeId = "3",
                DestinationScopeType = DestinationMarkupScopeTypes.Accommodation,
                SupplierCode = "jumeirah"
            },
            new MarkupPolicy
            {
                Id = 5,
                Value = 1,
                Currency = Currencies.USD,
                FunctionType = MarkupFunctionType.Percent,
                Description = "Markup 5",
                SubjectScopeId = "456",
                SubjectScopeType = SubjectMarkupScopeTypes.Agent,
                DestinationScopeId = string.Empty,
                DestinationScopeType = DestinationMarkupScopeTypes.Global,
                SupplierCode = "jumeirah"
            },
            new MarkupPolicy
            {
                Id = 6,
                Value = 1,
                Currency = Currencies.USD,
                FunctionType = MarkupFunctionType.Percent,
                Description = "Markup 6",
                SubjectScopeId = "RU",
                SubjectScopeType = SubjectMarkupScopeTypes.Country,
                DestinationScopeId = string.Empty,
                DestinationScopeType = DestinationMarkupScopeTypes.Global,
                SupplierCode = "jumeirah"
            }
        };


        private readonly Mock<EdoContext> _edoContextMock;
        private readonly ISupplierMarkupPolicyManager _supplierMarkupPolicyManager;
    }
}