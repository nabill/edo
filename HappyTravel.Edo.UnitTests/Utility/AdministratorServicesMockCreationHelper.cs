using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.AdministratorServices;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.NotificationCenter.Services;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Api.Services.Payments.Accounts;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Edo.Data.Markup;
using HappyTravel.Edo.Data.Payments;
using HappyTravel.Edo.UnitTests.Mocks;
using HappyTravel.Money.Enums;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Options;
using Moq;

namespace HappyTravel.Edo.UnitTests.Utility
{
    public class AdministratorServicesMockCreationHelper
    {
        public Mock<EdoContext> GetContextMock()
        {
            var edoContextMock = MockEdoContextFactory.Create();
            var strategy = new ExecutionStrategyMock();
            var dbFacade = new Mock<DatabaseFacade>(edoContextMock.Object);

            dbFacade.Setup(d => d.CreateExecutionStrategy()).Returns(strategy);
            edoContextMock.Setup(c => c.Database).Returns(dbFacade.Object);
            edoContextMock.Setup(x => x.Counterparties).Returns(DbSetMockProvider.GetDbSetMock(_counterparties));
            edoContextMock.Setup(x => x.Agencies).Returns(DbSetMockProvider.GetDbSetMock(_agencies));
            edoContextMock.Setup(x => x.Agents).Returns(DbSetMockProvider.GetDbSetMock(_agents));
            edoContextMock.Setup(x => x.AgentAgencyRelations).Returns(DbSetMockProvider.GetDbSetMock(_relations));
            edoContextMock.Setup(x => x.AgencyAccounts).Returns(DbSetMockProvider.GetDbSetMock(_agencyAccounts));
            edoContextMock.Setup(x => x.CounterpartyAccounts).Returns(DbSetMockProvider.GetDbSetMock(_counterpartyAccounts));
            edoContextMock.Setup(x => x.Countries).Returns(DbSetMockProvider.GetDbSetMock(_countries));
            edoContextMock.Setup(x => x.DisplayMarkupFormulas).Returns(DbSetMockProvider.GetDbSetMock(new List<DisplayMarkupFormula>()));

            return edoContextMock;
        }


        public CounterpartyManagementService GetCounterpartyManagementService(EdoContext context)
        {
            var options = new CounterpartyManagementMailingOptions(); 
            var mockOptions = new Mock<IOptions<CounterpartyManagementMailingOptions>>();
            mockOptions.Setup(o => o.Value).Returns(options);

            return new(context,
                Mock.Of<IManagementAuditService>(),
                Mock.Of<IDateTimeProvider>());
        }


        public AdminAgencyManagementService GetAgencyManagementService(EdoContext context)
        {
            return new(context,
                Mock.Of<IDateTimeProvider>(),
                Mock.Of<IManagementAuditService>());
        }


        public AgencyVerificationService GetAgencyVerificationService(EdoContext context)
        {
            var accountManagementServiceMock = new Mock<IAccountManagementService>();
            accountManagementServiceMock.Setup(am => am.CreateForCounterparty(It.IsAny<Counterparty>(), It.IsAny<Currencies>()))
                .Returns((Counterparty counterparty, Currencies currency) =>
                {
                    _counterpartyAccounts.Add(new CounterpartyAccount
                    {
                        CounterpartyId = counterparty.Id,
                        Currency = currency
                    });
                    return Task.FromResult(Result.Success());
                });
            accountManagementServiceMock.Setup(am => am.CreateForAgency(It.IsAny<Agency>(), It.IsAny<Currencies>()))
                .Returns((Agency agency, Currencies currency) =>
                {
                    _agencyAccounts.Add(new AgencyAccount
                    {
                        AgencyId = agency.Id,
                        Currency = Currencies.USD
                    });
                    return Task.FromResult(Result.Success());
                });

            var counterpartyManagementServiceMock = GetCounterpartyManagementService(context);

            var options = new CounterpartyManagementMailingOptions();
            var mockOptions = new Mock<IOptions<CounterpartyManagementMailingOptions>>();
            mockOptions.Setup(o => o.Value).Returns(options);

            var agentService = new Api.Services.Agents.AgentService(context, Mock.Of<IDateTimeProvider>());

            return new AgencyVerificationService(context, 
                accountManagementServiceMock.Object,
                Mock.Of<IManagementAuditService>(), 
                Mock.Of<INotificationService>(),
                mockOptions.Object,
                Mock.Of<IDateTimeProvider>(),
                agentService);
        }


        private readonly List<CounterpartyAccount> _counterpartyAccounts = new List<CounterpartyAccount>()
        {
            new CounterpartyAccount
            {
                Id = 1,
                CounterpartyId = 1,
                Currency = Currencies.AED,
                IsActive = true
            },
            new CounterpartyAccount
            {
                Id = 2,
                CounterpartyId = 2,
                Currency = Currencies.AED,
                IsActive = false
            }
        };

        private readonly List<AgencyAccount> _agencyAccounts = new List<AgencyAccount>
        {
            new AgencyAccount
            {
                Id = 1,
                AgencyId = 4,
                Currency = Currencies.EUR,
                IsActive = true
            },
            new AgencyAccount
            {
                Id = 2,
                AgencyId = 3,
                Currency = Currencies.EUR,
                IsActive = false
            }
        };

        private static readonly IEnumerable<Agent> _agents = new[]
        {
            new Agent
            {
                Id = 1,
                Email = "email",
                FirstName = "fn",
                LastName = "ln",
                Position = "pos",
                Title = "title",
            },
            new Agent
            {
                Id = 2,
                Email = "email2",
                FirstName = "fn2",
                LastName = "ln2",
                Position = "pos2",
                Title = "title2",
            },
            new Agent
            {
                Id = 3,
                Email = "email3",
                FirstName = "fn3",
                LastName = "ln3",
                Position = "pos3",
                Title = "title3",
            },
            new Agent
            {
                Id = 4,
                Email = "email4",
                FirstName = "fn4",
                LastName = "ln4",
                Position = "pos4",
                Title = "title4",
            },
            new Agent
            {
                Id = 5,
                Email = "email5",
                FirstName = "fn5",
                LastName = "ln5",
                Position = "pos5",
                Title = "title5",
            },
            new Agent
            {
                Id = 6,
                Email = "email6",
                FirstName = "fn6",
                LastName = "ln6",
                Position = "pos6",
                Title = "title6",
            },
            new Agent
            {
                Id = 14,
                FirstName = "Prediction",
                LastName = "Example",
                Email = "agentexample@mail.com",
            },
            new Agent
            {
                Id = 15,
                FirstName = "Prediction1",
                LastName = "Example1",
                Email = "agentexample1@mail.com",
            },
            new Agent
            {
                Id = 20,
                FirstName = "Agent20FirstName",
                LastName = "Agent20LastName",
                Email = "agent20@mail.com"
            }
        };

        private readonly IEnumerable<Counterparty> _counterparties = new[]
        {
            new Counterparty
            {
                Id = 1,
                Name = "Test",
                Address = "Test address",
                City = "Test city",
                CountryCode = "AF",
                Phone = "+7 111 1111111"
            },
            new Counterparty
            {
                Id = 2,
                Name = "Test1",
                Address = "Test address 2",
                City = "Test city 2",
                CountryCode = "AF",
                Phone = "+7 222 2222222"
            },
            new Counterparty
            {
                Id = 3,
                Name = "Test",
                Address = "Test address 3",
                City = "Test city 3",
                CountryCode = "AF",
                Phone = "+7 333 3333333"
            },
            new Counterparty
            {
                Id = 14,
                Name = "CounterpartyWithBillingEmail",
                Address = "Test address 4",
                City = "Test city 4",
                CountryCode = "AF",
                Phone = "+7 444 4444444"
            },
            new Counterparty
            {
                Id = 15,
                Name = "CounterpartyWithoutBillingEmail",
                Address = "Test address 5",
                City = "Test city 5",
                CountryCode = "AF",
                Phone = "+7 555 5555555"
            }
        };

        private readonly IEnumerable<Agency> _agencies = new[]
        {
            new Agency
            {
                Id = 1,
                CounterpartyId = 1,
                VerificationState = AgencyVerificationStates.PendingVerification,
                Name = "agencyName",
                CountryCode = "AF",
                IsActive = true
            },
            new Agency
            {
                Id = 2,
                CounterpartyId = 1,
                Name = "agencyName2",
                CountryCode = "AF",
                ParentId = 1,
                IsActive = true,
                Ancestors = new List<int>{1},
            },
            new Agency
            {
                Id = 3,
                CounterpartyId = 2,
                VerificationState = AgencyVerificationStates.PendingVerification,
                Name = "agencyName3",
                CountryCode = "AF",
                IsActive = false
            },
            new Agency
            {
                Id = 5,
                CounterpartyId = 2,
                Name = "childAgency",
                CountryCode = "AF",
                IsActive = false,
                ParentId = 3,
                Ancestors = new List<int>{3},
            },
            new Agency
            {
                Id = 4,
                CounterpartyId = 1,
                Name = "childAgency",
                CountryCode = "AF",
                ParentId = 1,
                IsActive = true,
                Ancestors = new List<int>{1},
            },
            new Agency
            {
                Id = 14,
                CounterpartyId = 14,
                VerificationState = AgencyVerificationStates.FullAccess,
                Name = "AgencyExampleForPredictions",
                BillingEmail = "predictionsExample@mail.com",
                CountryCode = "AF",
                IsActive = true
            },
            new Agency
            {
                Id = 15,
                CounterpartyId = 15,
                VerificationState = AgencyVerificationStates.FullAccess,
                Name = "AgencyExampleForPredictions1",
                CountryCode = "AF",
                IsActive = true
            },
            new Agency
            {
                Id = 20,
                CounterpartyId = 3,
                VerificationState = AgencyVerificationStates.ReadOnly,
                Name = "RootAgencyForCounterparty3",
                CountryCode = "AF",
                IsActive = true
            }
        };

        private readonly IEnumerable<AgentAgencyRelation> _relations = new[]
        {
            new AgentAgencyRelation
            {
                AgencyId = 1,
                AgentId = 1,
                Type = AgentAgencyRelationTypes.Master,
                IsActive = true
            },
            new AgentAgencyRelation
            {
                AgencyId = 1,
                AgentId = 2,
                Type = AgentAgencyRelationTypes.Regular,
                IsActive = true
            },
            new AgentAgencyRelation
            {
                AgencyId = 2,
                AgentId = 4,
                Type = AgentAgencyRelationTypes.Regular,
                IsActive = true
            },
            new AgentAgencyRelation
            {
                AgencyId = 3,
                AgentId = 5,
                Type = AgentAgencyRelationTypes.Master,
                IsActive = false
            },
            new AgentAgencyRelation
            {
                AgencyId = 3,
                AgentId = 6,
                Type = AgentAgencyRelationTypes.Regular,
                IsActive = false
            },
            new AgentAgencyRelation
            {
                AgencyId = 14,
                AgentId = 14,
                Type = AgentAgencyRelationTypes.Master,
                IsActive = true
            },
            new AgentAgencyRelation
            {
                AgencyId = 15,
                AgentId = 15,
                Type = AgentAgencyRelationTypes.Master,
                IsActive = true
            },
            new AgentAgencyRelation
            {
                AgencyId = 20,
                AgentId = 20,
                Type = AgentAgencyRelationTypes.Master,
                IsActive = true
            }
        };

        private readonly IEnumerable<Data.Locations.Country> _countries = new[]
        {
            new Data.Locations.Country
            {
                Code = "AF",
                Names =
                    "{\"ar\": \"أفغانستان\", \"cn\": \"阿富汗\", \"en\": \"Afghanistan\", \"es\": \"Afganistán\", \"fr\": \"Afghanistan\", \"ru\": \"Афганистан\"}"
            },
        };
    }
}