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
using HappyTravel.Edo.Data.Payments;
using HappyTravel.Edo.UnitTests.Infrastructure.DbSetMocks;
using HappyTravel.EdoContracts.General.Enums;
using HappyTravel.Money.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Moq;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Administrator
{
    public class CounterpartyManagementTests
    {
        public CounterpartyManagementTests(Mock<EdoContext> edoContextMock)
        {
            var strategy = new ExecutionStrategyMock();
            var dbFacade = new Mock<DatabaseFacade>(edoContextMock.Object);
            dbFacade.Setup(d => d.CreateExecutionStrategy()).Returns(strategy);
            edoContextMock.Setup(c => c.Database).Returns(dbFacade.Object);

            edoContextMock.Setup(x => x.Counterparties).Returns(DbSetMockProvider.GetDbSetMock(_counterparties));
            edoContextMock.Setup(x => x.Agencies).Returns(DbSetMockProvider.GetDbSetMock(_agencies));
            edoContextMock.Setup(x => x.Agents).Returns(DbSetMockProvider.GetDbSetMock(_agents));
            edoContextMock.Setup(x => x.AgentAgencyRelations).Returns(DbSetMockProvider.GetDbSetMock(_relations));
            edoContextMock.Setup(x => x.PaymentAccounts).Returns(DbSetMockProvider.GetDbSetMock(_paymentAccounts));
            edoContextMock.Setup(x => x.CounterpartyAccounts).Returns(DbSetMockProvider.GetDbSetMock(_counterpartyAccounts));

            _context = edoContextMock.Object;

            var permissionsManagementMock = new Mock<IAgentPermissionManagementService>();
            permissionsManagementMock.Setup(p => p.SetInAgencyPermissions(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<InAgencyPermissions>()))
                .Returns((int agencyId, int agentId, InAgencyPermissions permissions) =>
                {
                    var relations = _relations.Where(r
                        => r.AgencyId == agencyId && r.AgentId == agentId && r.InAgencyPermissions.HasFlag(InAgencyPermissions.PermissionManagement)).ToList();
                    foreach (var rel in relations)
                        rel.InAgencyPermissions = permissions;
                    return Task.FromResult(Result.Ok(new List<InAgencyPermissions>()));
                });

            var accountManagementServiceMock = new Mock<IAccountManagementService>();
            accountManagementServiceMock.Setup(am => am.CreateForCounterparty(It.IsAny<Counterparty>(), It.IsAny<Currencies>()))
                .Returns((Counterparty counterparty, Currencies currency) =>
                {
                    _counterpartyAccounts.Add(new CounterpartyAccount
                    {
                        CounterpartyId = counterparty.Id,
                        Currency = currency
                    });
                    return Task.FromResult(Result.Ok());
                });
            accountManagementServiceMock.Setup(am => am.CreateForAgency(It.IsAny<Agency>(), It.IsAny<Currencies>()))
                .Returns((Agency agency, Currencies currency) =>
                {
                    _paymentAccounts.Add(new PaymentAccount
                    {
                        AgencyId = agency.Id,
                        Currency = Currencies.USD
                    });
                    return Task.FromResult(Result.Ok());
                });

            _counterpartyManagementService = new CounterpartyManagementService(_context,
                Mock.Of<IDateTimeProvider>(),
                Mock.Of<IManagementAuditService>(),
                permissionsManagementMock.Object,
                accountManagementServiceMock.Object);
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


        [Fact]
        public async Task Verify_as_full_accessed_should_update_permissions()
        {
            var (_, isFailure, error) = await _counterpartyManagementService.VerifyAsFullyAccessed(1, "Test reason");
            var agencies = _context.Agencies.Where(a => a.CounterpartyId == 1).ToList();
            var relations = (from r in _context.AgentAgencyRelations
                join ag in agencies
                    on r.AgencyId equals ag.Id
                select r).ToList();
            var counterparty = _context.Counterparties.Single(c => c.Id == 1);

            Assert.False(isFailure);
            Assert.True(counterparty.State == CounterpartyStates.FullAccess && counterparty.VerificationReason.Contains("Test reason"));
            Assert.True(relations.All(r
                => r.InAgencyPermissions.HasFlag(InAgencyPermissions.PermissionManagement) && r.Type == AgentAgencyRelationTypes.Master
                    ? r.InAgencyPermissions == PermissionSets.FullAccessMaster
                    : r.InAgencyPermissions == PermissionSets.FullAccessDefault));
        }


        [Fact]
        public async Task Verify_as_read_only_should_update_permissions()
        {
            var (_, isFailure, error) = await _counterpartyManagementService.VerifyAsReadOnly(1, "Test reason");
            var agencies = _context.Agencies.Where(a => a.CounterpartyId == 1).ToList();
            var relations = (from r in _context.AgentAgencyRelations
                join ag in agencies
                    on r.AgencyId equals ag.Id
                select r).ToList();
            var counterparty = _context.Counterparties.Single(c => c.Id == 1);

            Assert.False(isFailure);
            Assert.True(counterparty.State == CounterpartyStates.ReadOnly && counterparty.VerificationReason.Contains("Test reason"));
            Assert.True(relations.All(r
                => r.InAgencyPermissions.HasFlag(InAgencyPermissions.PermissionManagement) && r.Type == AgentAgencyRelationTypes.Master
                    ? r.InAgencyPermissions == PermissionSets.ReadOnlyMaster
                    : r.InAgencyPermissions == PermissionSets.ReadOnlyDefault));
            Assert.True(_context.CounterpartyAccounts.SingleOrDefaultAsync(c => c.CounterpartyId == 1) != null);
            Assert.True(agencies.All(a => _context.PaymentAccounts.Any(ac => ac.AgencyId == a.Id)));
        }


        private readonly List<CounterpartyAccount> _counterpartyAccounts = new List<CounterpartyAccount>();
        private readonly List<PaymentAccount> _paymentAccounts = new List<PaymentAccount>();

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
                InAgencyPermissions = InAgencyPermissions.ObserveMarkup | InAgencyPermissions.PermissionManagement
            },
            new AgentAgencyRelation
            {
                AgencyId = 1,
                AgentId = 2,
                Type = AgentAgencyRelationTypes.Regular,
                InAgencyPermissions = InAgencyPermissions.PermissionManagement
            },
            new AgentAgencyRelation
            {
                AgencyId = 2,
                AgentId = 4,
                Type = AgentAgencyRelationTypes.Regular,
                InAgencyPermissions = InAgencyPermissions.PermissionManagement
            }
        };

        private readonly EdoContext _context;
        private readonly ICounterpartyManagementService _counterpartyManagementService;
    }
}