using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Services.AdministratorServices;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Api.Services.Payments.Accounts;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Edo.Data.Payments;
using HappyTravel.Edo.UnitTests.Infrastructure;
using HappyTravel.Edo.UnitTests.Infrastructure.DbSetMocks;
using HappyTravel.Money.Enums;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Moq;

namespace HappyTravel.Edo.UnitTests.Administrator.CounterpartyManagement
{
    public class MockCreationHelper
    {
        public Mock<EdoContext> GetContextMock()
        {
            var edoContextMock = MockEdoContext.Create();
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
            return edoContextMock;
        }


        public CounterpartyManagementService GetCounterpartyManagementService(EdoContext context)
        {
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

            return new CounterpartyManagementService(context,
                Mock.Of<IDateTimeProvider>(),
                Mock.Of<IManagementAuditService>(),
                permissionsManagementMock.Object,
                accountManagementServiceMock.Object);
        }


        private readonly List<CounterpartyAccount> _counterpartyAccounts = new List<CounterpartyAccount>();
        private readonly List<PaymentAccount> _paymentAccounts = new List<PaymentAccount>();

        private static readonly IEnumerable<Agent> _agents = new[]
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
                Name = "agencyName"
            },
            new Agency
            {
                Id = 2,
                CounterpartyId = 1,
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
    }
}