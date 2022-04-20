using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Markups;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Api.Services.Markups;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Common.Enums.Administrators;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Edo.Data.Locations;
using HappyTravel.Edo.Data.Management;
using HappyTravel.Edo.Data.Markup;
using HappyTravel.Edo.UnitTests.Mocks;
using HappyTravel.Edo.UnitTests.Utility;
using HappyTravel.Money.Enums;
using HappyTravel.MultiLanguage;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Moq;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Markups.AdminMarkupPolicyTests
{
    public class AdminMarkupPolicyManagerTests
    {
        public AdminMarkupPolicyManagerTests()
        {
            _edoContextMock = MockEdoContextFactory.Create();
            SetupInitialData();

            var tokenInfoAccessorMock = new Mock<ITokenInfoAccessor>();
            tokenInfoAccessorMock.Setup(x => x.GetIdentity())
                .Returns("hash");

            var administratorContext = new HttpBasedAdministratorContext(_edoContextMock.Object, tokenInfoAccessorMock.Object);
            var auditServiceMock = new Mock<IMarkupPolicyAuditService>();
            auditServiceMock.Setup(a => a.Write<It.IsAnyType>(It.IsAny<MarkupPolicyEventType>(), It.IsAny<It.IsAnyType>(), It.IsAny<ApiCaller>()))
                .Returns(Task.CompletedTask);


            _adminMarkupPolicyManager = new AdminMarkupPolicyManager(_edoContextMock.Object,
                Mock.Of<IDateTimeProvider>(), Mock.Of<IDisplayedMarkupFormulaService>(),
                administratorContext, auditServiceMock.Object);

            var strategy = new ExecutionStrategyMock();

            var dbFacade = new Mock<DatabaseFacade>(_edoContextMock.Object);
            dbFacade.Setup(d => d.CreateExecutionStrategy()).Returns(strategy);
            _edoContextMock.Setup(c => c.Database).Returns(dbFacade.Object);
        }


        [Fact]
        public async Task Add_location_markup_with_destinaion_data_should_return_fail()
        {
            var settings = new MarkupPolicySettings("Description", MarkupFunctionType.Percent, 2, Currencies.USD,
                "2", "Country_12512", SubjectMarkupScopeTypes.Market, DestinationMarkupScopeTypes.Country);

            var (_, isFailure, error) = await _adminMarkupPolicyManager.AddLocationPolicy(settings);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Add_location_or_destination_markup_with_wrong_value_should_return_fail()
        {
            var settings = new MarkupPolicySettings("Description", MarkupFunctionType.Percent, 0, Currencies.USD,
                null, "4", null, DestinationMarkupScopeTypes.Market);

            var (_, isFailure, error) = await _adminMarkupPolicyManager.AddLocationPolicy(settings);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Add_destination_markup_with_wrong_market_should_return_fail()
        {
            var settings = new MarkupPolicySettings("Description", MarkupFunctionType.Percent, -1, Currencies.USD,
                null, "5", null, DestinationMarkupScopeTypes.Market);

            var (_, isFailure, error) = await _adminMarkupPolicyManager.AddLocationPolicy(settings);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Add_location_markup_with_wrong_market_should_return_fail()
        {
            var settings = new MarkupPolicySettings("Description", MarkupFunctionType.Percent, -1, Currencies.USD,
                "5", null, SubjectMarkupScopeTypes.Market, null);

            var (_, isFailure, error) = await _adminMarkupPolicyManager.AddLocationPolicy(settings);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Add_location_markup_with_unexpected_type_should_return_fail()
        {
            var settings = new MarkupPolicySettings("Description", MarkupFunctionType.Percent, -1, Currencies.USD,
                "4", null, SubjectMarkupScopeTypes.Agent, null);

            var (_, isFailure, error) = await _adminMarkupPolicyManager.AddLocationPolicy(settings);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Add_destination_markup_with_unexpected_type_should_return_fail()
        {
            var settings = new MarkupPolicySettings("Description", MarkupFunctionType.Percent, -1, Currencies.USD,
                null, "4", null, DestinationMarkupScopeTypes.Accommodation);

            var (_, isFailure, error) = await _adminMarkupPolicyManager.AddLocationPolicy(settings);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Add_destination_agency_markup_which_already_exist_should_return_fail()
        {
            var settings = new MarkupPolicySettings("Description", MarkupFunctionType.Percent, -1, Currencies.USD,
                "1", "1", SubjectMarkupScopeTypes.Agency, DestinationMarkupScopeTypes.Market);

            var (_, isFailure, error) = await _adminMarkupPolicyManager.AddLocationPolicy(settings);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Add_destination_agency_markup_with_unexpected_type_should_return_fail()
        {
            var settings = new MarkupPolicySettings("Description", MarkupFunctionType.Percent, -1, Currencies.USD,
                "1", "4", SubjectMarkupScopeTypes.Agency, DestinationMarkupScopeTypes.Accommodation);

            var (_, isFailure, error) = await _adminMarkupPolicyManager.AddLocationPolicy(settings);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Add_destination_agency_markup_with_null_agencyId_should_return_fail()
        {
            var settings = new MarkupPolicySettings("Description", MarkupFunctionType.Percent, -1, Currencies.USD,
                null, "4", SubjectMarkupScopeTypes.Agency, DestinationMarkupScopeTypes.Market);

            var (_, isFailure, error) = await _adminMarkupPolicyManager.AddLocationPolicy(settings);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Add_destination_agency_markup_with_wrong_agencyId_should_return_fail()
        {
            var settings = new MarkupPolicySettings("Description", MarkupFunctionType.Percent, -1, Currencies.USD,
                "4", "4", SubjectMarkupScopeTypes.Agency, DestinationMarkupScopeTypes.Market);

            var (_, isFailure, error) = await _adminMarkupPolicyManager.AddLocationPolicy(settings);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Add_destinaion_markup_already_exists_should_return_fail()
        {
            var settings = new MarkupPolicySettings("Description", MarkupFunctionType.Percent, 2, Currencies.USD,
                null, "2", null, DestinationMarkupScopeTypes.Market);

            var (_, isFailure, error) = await _adminMarkupPolicyManager.AddLocationPolicy(settings);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Modify_location_markup_with_wrong_policyId_should_return_fail()
        {
            var settings = new MarkupPolicySettings("Description", MarkupFunctionType.Percent, 2, Currencies.USD,
                null, null, null, null);

            var (_, isFailure, error) = await _adminMarkupPolicyManager.ModifyLocationPolicy(6, settings);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Modify_location_or_destination_markup_with_unupdated_data_should_return_fail()
        {
            var settings = new MarkupPolicySettings("Description", MarkupFunctionType.Percent, 2, Currencies.USD,
                "2", "Country_12512", SubjectMarkupScopeTypes.Market, DestinationMarkupScopeTypes.Country);

            var (_, isFailure, error) = await _adminMarkupPolicyManager.ModifyLocationPolicy(1, settings);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Modify_location_or_destination_markup_with_wrong_value_should_return_fail()
        {
            var settings = new MarkupPolicySettings("Description", MarkupFunctionType.Percent, 0, Currencies.USD,
                null, null, null, null);

            var (_, isFailure, error) = await _adminMarkupPolicyManager.ModifyLocationPolicy(1, settings);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Modify_location_markup_with_not_location_policyId_should_return_fail()
        {
            var settings = new MarkupPolicySettings("Description", MarkupFunctionType.Percent, -1, Currencies.USD,
                null, null, null, null);

            var (_, isFailure, error) = await _adminMarkupPolicyManager.ModifyLocationPolicy(5, settings);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Modify_destination_markup_with_not_location_policyId_should_return_fail()
        {
            var settings = new MarkupPolicySettings("Description", MarkupFunctionType.Percent, -1, Currencies.USD,
                null, null, null, null);

            var (_, isFailure, error) = await _adminMarkupPolicyManager.ModifyLocationPolicy(4, settings);

            Assert.True(isFailure);
        }


        private void SetupInitialData()
        {
            _edoContextMock
                .Setup(c => c.Markets)
                .Returns(DbSetMockProvider.GetDbSetMock(markets));

            _edoContextMock
                .Setup(c => c.Agencies)
                .Returns(DbSetMockProvider.GetDbSetMock(agencies));

            _edoContextMock
                .Setup(c => c.MarkupPolicies)
                .Returns(DbSetMockProvider.GetDbSetMock(markupPolicies));

            _edoContextMock
                .Setup(x => x.Administrators)
                .Returns(DbSetMockProvider.GetDbSetMock(administrators));
            _edoContextMock
                .Setup(x => x.AdministratorRoles)
                .Returns(DbSetMockProvider.GetDbSetMock(administratorRoles));
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
                DestinationScopeType = DestinationMarkupScopeTypes.Market
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
                DestinationScopeId = "2",
                DestinationScopeType = DestinationMarkupScopeTypes.Market
            },
            new MarkupPolicy
            {
                Id = 3,
                Value = 1,
                Currency = Currencies.USD,
                FunctionType = MarkupFunctionType.Percent,
                Description = "Markup 3",
                SubjectScopeId = string.Empty,
                SubjectScopeType = SubjectMarkupScopeTypes.Global,
                DestinationScopeId = "3",
                DestinationScopeType = DestinationMarkupScopeTypes.Market
            },
            new MarkupPolicy
            {
                Id = 4,
                Value = 1,
                Currency = Currencies.USD,
                FunctionType = MarkupFunctionType.Percent,
                Description = "Markup 3",
                SubjectScopeId = string.Empty,
                SubjectScopeType = SubjectMarkupScopeTypes.Global,
                DestinationScopeId = "3",
                DestinationScopeType = DestinationMarkupScopeTypes.Accommodation
            },
            new MarkupPolicy
            {
                Id = 5,
                Value = 1,
                Currency = Currencies.USD,
                FunctionType = MarkupFunctionType.Percent,
                Description = "Markup 3",
                SubjectScopeId = "456",
                SubjectScopeType = SubjectMarkupScopeTypes.Agent,
                DestinationScopeId = string.Empty,
                DestinationScopeType = DestinationMarkupScopeTypes.Global
            }
        };


        private readonly IEnumerable<Administrator> administrators = new[]
        {
            new Administrator
            {
                Id = 0,
                AdministratorRoleIds = new []{0},
                IdentityHash = HashGenerator.ComputeSha256("hash"),
                IsActive = true
            }
        };


        private readonly IEnumerable<AdministratorRole> administratorRoles = new[]
        {
            new AdministratorRole
            {
                Id = 0,
                Permissions = AdministratorPermissions.AccountReplenish | AdministratorPermissions.AdministratorInvitation
            }
        };


        private readonly Mock<EdoContext> _edoContextMock;
        private readonly IAdminMarkupPolicyManager _adminMarkupPolicyManager;
    }
}