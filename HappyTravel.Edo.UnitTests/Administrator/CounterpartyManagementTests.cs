using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.AdministratorServices;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Api.Services.Payments.Accounts;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Edo.Data.Markup;
using HappyTravel.Edo.UnitTests.Infrastructure.DbSetMocks;
using HappyTravel.EdoContracts.General.Enums;
using HappyTravel.Money.Enums;
using Moq;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Administrator
{
    public class CounterpartyManagementTests
    {
        public CounterpartyManagementTests(Mock<EdoContext> edoContextMock)
        {
            edoContextMock.Setup(x => x.Counterparties).Returns(DbSetMockProvider.GetDbSetMock(_counterparties));
            edoContextMock.Setup(x => x.Agencies).Returns(DbSetMockProvider.GetDbSetMock(_agencies));
            edoContextMock.Setup(x => x.Agents).Returns(DbSetMockProvider.GetDbSetMock(_agents));
            edoContextMock.Setup(x => x.AgentAgencyRelations).Returns(DbSetMockProvider.GetDbSetMock(_relations));
            edoContextMock.Setup(x => x.MarkupPolicies).Returns(DbSetMockProvider.GetDbSetMock(new List<MarkupPolicy>()));

            _context = edoContextMock.Object;
            _counterpartyManagementService = new CounterpartyManagementService(_context,
                Mock.Of<IDateTimeProvider>(),
                Mock.Of<IManagementAuditService>(),
                Mock.Of<IAgentPermissionManagementService>(),
                Mock.Of<IAccountManagementService>());
        }


        [Fact]
        public async Task Get_specified_counterparty_should_return_counterparty_infо()
        {
            var (_, isFailure, counterparty, error) = await _counterpartyManagementService.Get(1);
            Assert.False(isFailure);
            Assert.True(!counterparty.Equals(default) && counterparty.Name == "Test");
        }


        [Fact]
        public async Task Get_not_existed_counterparty_should_fail()
        {
            var (_, isFailure, counterparty, error) = await _counterpartyManagementService.Get(7);
            Assert.True(isFailure);
        }


        [Fact]
        public async Task Get_counterparty_should_return_counterparty_list()
        {
            var counterpartyList = await _counterpartyManagementService.Get();
            Assert.True(counterpartyList.Count > 0);
        }


        [Fact]
        public async Task Get_counterparty_agencies_should_return_agencies()
        {
            var (_, isFailure, agencies, error) = await _counterpartyManagementService.GetAllCounterpartyAgencies(1);
            Assert.False(isFailure);
            Assert.True(agencies.Count > 0);
        }


        [Fact]
        public async Task Get_not_existed_counterparty_agencies_should_fail()
        {
            var (_, isFailure, _, _) = await _counterpartyManagementService.GetAllCounterpartyAgencies(7);
            Assert.True(isFailure);
        }


        [Fact]
        public async Task Counterparty_update_should_pass()
        {
            var counterpartyToUpdate = new CounterpartyInfo(
                name: "RenamedName",
                address: "changed address",
                countryCode: "changedCode",
                city: "changed city",
                phone: "79265748556",
                fax: "79265748336",
                postalCode: "changed code",
                preferredCurrency: Currencies.EUR,
                preferredPaymentMethod: PaymentMethods.Offline,
                website: "changed website",
                vatNumber: "changed vatNumber",
                billingEmail: "changed email"
            );

            var (_, isFailure, counterparty, error) = await _counterpartyManagementService.Update(counterpartyToUpdate, 1);
            Assert.False(isFailure);
            Assert.True(counterparty.Equals(counterpartyToUpdate));
            Assert.True(_context.Counterparties.Single(c => c.Id == 1).Name == counterpartyToUpdate.Name);
        }


        [Fact]
        public async Task Update_not_existing_counterparty_should_fail()
        {
            var counterpartyToUpdate = new CounterpartyInfo();
            var (_, isFailure, _, _) = await _counterpartyManagementService.Update(counterpartyToUpdate, 1);
            Assert.True(isFailure);
        }


        private readonly IEnumerable<Agent> _agents = new[]
        {
            new Agent
            {
                Id = 1,
                Email = "email",
                FirstName = "fn",
                LastName = "ln",
                Position = "pos",
                Title = "title"
            },
            new Agent
            {
                Id = 2,
                Email = "email2",
                FirstName = "fn2",
                LastName = "ln2",
                Position = "pos2",
                Title = "title2"
            },
            new Agent
            {
                Id = 3,
                Email = "email3",
                FirstName = "fn3",
                LastName = "ln3",
                Position = "pos3",
                Title = "title3"
            },
            new Agent
            {
                Id = 4,
                Email = "email4",
                FirstName = "fn4",
                LastName = "ln4",
                Position = "pos4",
                Title = "title4"
            },
        };

        private readonly IEnumerable<Counterparty> _counterparties = new[]
        {
            new Counterparty
            {
                Id = 1,
                Name = "Test"
            },
            new Counterparty
            {
                Id = 2,
                Name = "Test1"
            }
        };

        private readonly IEnumerable<Agency> _agencies = new[]
        {
            new Agency
            {
                Id = 1,
                CounterpartyId = 1,
                IsDefault = true,
                Name = "agencyName"
            },
            new Agency
            {
                Id = 2,
                CounterpartyId = 1,
                IsDefault = false,
                Name = "agencyName2"
            }
        };

        private readonly IEnumerable<AgentAgencyRelation> _relations = new[]
        {
            new AgentAgencyRelation
            {
                AgencyId = 1,
                AgentId = 1,
                Type = AgentAgencyRelationTypes.Master,
                InAgencyPermissions = InAgencyPermissions.ObserveMarkup
            },
            new AgentAgencyRelation
            {
                AgencyId = 1,
                AgentId = 2,
                Type = AgentAgencyRelationTypes.Regular
            },
            new AgentAgencyRelation
            {
                AgencyId = 2,
                AgentId = 4,
                Type = AgentAgencyRelationTypes.Regular
            }
        };

        private readonly EdoContext _context;
        private readonly ICounterpartyManagementService _counterpartyManagementService;
    }
}