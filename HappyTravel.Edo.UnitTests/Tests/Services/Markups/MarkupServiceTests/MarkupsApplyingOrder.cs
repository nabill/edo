using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Accommodations.Availability;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.CurrencyConversion;
using HappyTravel.Edo.Api.Services.Markups;
using HappyTravel.Edo.Api.Services.Markups.Abstractions;
using HappyTravel.Edo.Api.Services.Markups.Templates;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Edo.Data.Markup;
using HappyTravel.Edo.UnitTests.Mocks;
using HappyTravel.Edo.UnitTests.Utility;
using HappyTravel.Money.Enums;
using HappyTravel.Money.Models;
using Microsoft.Extensions.Options;
using Moq;
using NetTopologySuite.Utilities;
using Xunit;
using Assert = Xunit.Assert;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Markups.MarkupServiceTests
{
    public class MarkupsApplyingOrder : IDisposable
    {
        public MarkupsApplyingOrder()
        {
            var allPolicies = _agentPolicies
                .Union(_counterpartyPolicies)
                .Union(_globalPolicies)
                .Union(_agencyPolicies)
                .Union(_agencyLocalityPolicies)
                .ToList();

            var edoContextMock = MockEdoContextFactory.Create();
            edoContextMock.Setup(c => c.Agencies)
                .Returns(DbSetMockProvider.GetDbSetMock(_agencies));
            
            var currencyRateServiceMock = new Mock<ICurrencyRateService>();
            currencyRateServiceMock
                .Setup(c => c.Get(It.IsAny<Currencies>(), It.IsAny<Currencies>()))
                .Returns(new ValueTask<Result<decimal>>(Result.Success((decimal)1)));

            var markupPolicyStorage = new MarkupPolicyStorage(Mock.Of<IOptionsMonitor<MarkupPolicyStorageOptions>>(_ => _.CurrentValue ==
                new MarkupPolicyStorageOptions {Timeout = TimeSpan.FromMilliseconds(1)}));
            markupPolicyStorage.Set(allPolicies);
    
            _markupPolicyService = new MarkupPolicyService(markupPolicyStorage);

            var discountServiceMock = new  Mock<IDiscountFunctionService>();
            discountServiceMock.Setup(service => service.Get(It.IsAny<MarkupPolicy>(), It.IsAny<MarkupSubjectInfo>()))
                .ReturnsAsync((price => new ValueTask<MoneyAmount>(price)));
            
            _markupService = new MarkupService(_markupPolicyService, discountServiceMock.Object, new MarkupPolicyTemplateService(), currencyRateServiceMock.Object, new FakeMemoryFlow());
        }
    
    
        [Fact]
        public void Policies_should_be_ordered_by_scope()
        {
            var policies = _markupPolicyService.Get(MarkupSubject, MarkupObject, MarkupPolicyTarget.AccommodationAvailability);
            for (var i = 0; i < policies.Count - 1; i++)
            {
                Assert.True(ScopeOrderIsCorrect(policies[i].AgentScopeType, policies[i + 1].AgentScopeType));
            }
    
            bool ScopeOrderIsCorrect(AgentMarkupScopeTypes firstScope, AgentMarkupScopeTypes secondScope)
            {
                switch (firstScope)
                {
                    case AgentMarkupScopeTypes.Global:
                    {
                        return true;
                    }
                    case AgentMarkupScopeTypes.Counterparty:
                    {
                        return secondScope != AgentMarkupScopeTypes.Global;
                    }
                    case AgentMarkupScopeTypes.Agency:
                    {
                        return secondScope != AgentMarkupScopeTypes.Global &&
                            secondScope != AgentMarkupScopeTypes.Counterparty;
                    }
                    case AgentMarkupScopeTypes.Agent:
                    {
                        return secondScope != AgentMarkupScopeTypes.Global &&
                            secondScope != AgentMarkupScopeTypes.Counterparty &&
                            secondScope != AgentMarkupScopeTypes.Agency;
                    }
                    case AgentMarkupScopeTypes.Location:
                    {
                        return secondScope != AgentMarkupScopeTypes.Global &&
                            secondScope != AgentMarkupScopeTypes.Counterparty &&
                            secondScope != AgentMarkupScopeTypes.Agency &&
                            secondScope != AgentMarkupScopeTypes.Agent;
                    }
                    default: throw new AssertionFailedException("Unexpected scope type");
                }
            }
        }
        
        [Fact]
        public void Policies_in_scope_should_be_ordered_by_order()
        {
            var policies = _markupPolicyService.Get(MarkupSubject, MarkupObject, MarkupPolicyTarget.AccommodationAvailability);
            for (var i = 0; i < policies.Count - 1; i++)
            {
                Assert.True(ScopeOrderIsCorrect(policies[i], policies[i + 1]));
            }
    
            bool ScopeOrderIsCorrect(MarkupPolicy firstPolicy, MarkupPolicy secondPolicy)
            {
                if (firstPolicy.AgentScopeType != secondPolicy.AgentScopeType)
                    return true;

                if (firstPolicy.AgentScopeType == AgentMarkupScopeTypes.Agency &&
                    secondPolicy.AgentScopeType == AgentMarkupScopeTypes.Agency &&
                    firstPolicy.AgentScopeId != secondPolicy.AgentScopeId)
                {
                    return true;
                }
                
                return firstPolicy.Order < secondPolicy.Order;
            }
        }


        [Fact]
        public void Agencies_policies_should_be_ordered_by_agency_tree()
        {
            var policies = _markupPolicyService.Get(MarkupSubject, MarkupObject, MarkupPolicyTarget.AccommodationAvailability);
            var agencyPolicies = policies.Where(p => p.AgentScopeType == AgentMarkupScopeTypes.Agency).ToList();
            
            for (var i = 0; i < agencyPolicies.Count - 1; i++)
            {
                Assert.True(int.Parse(agencyPolicies[i].AgentScopeId) == MarkupSubject.AgencyAncestors[i]);
            }
        }
    
        [Theory]
        [InlineData(100, Currencies.EUR, 4206528645)]
        [InlineData(240.5, Currencies.USD, 10107528645.0)]
        [InlineData(0.13, Currencies.USD, 11988645.00)]
        public async Task Policies_calculation_should_execute_in_right_order(decimal supplierPrice, Currencies currency, decimal expectedResultPrice)
        {
            var data = new TestStructureUnderMarkup {Price = new MoneyAmount(supplierPrice, currency)};
            
            var dataWithMarkup = await _markupService.ApplyMarkups(MarkupSubject, MarkupObject, data, TestStructureUnderMarkup.Apply);
            
            Assert.Equal(expectedResultPrice, dataWithMarkup.Price.Amount);
        }
    
        private readonly IEnumerable<MarkupPolicy> _agentPolicies = new[]
        {
            new MarkupPolicy
            {
                Id = 1,
                Order = 21,
                Target = MarkupPolicyTarget.AccommodationAvailability,
                AgentScopeType = AgentMarkupScopeTypes.Agent,
                AgentScopeId = $"{MarkupSubject.AgencyId}-{MarkupSubject.AgentId}",
                TemplateId = 2,
                TemplateSettings = new Dictionary<string, decimal> {{"addition", 2}},
            },
            new MarkupPolicy
            {
                Id = 2,
                AgentScopeId = $"{MarkupSubject.AgencyId}-{MarkupSubject.AgentId}",
                Order = 1,
                Target = MarkupPolicyTarget.AccommodationAvailability,
                AgentScopeType = AgentMarkupScopeTypes.Agent,
                TemplateId = 1,
                TemplateSettings = new Dictionary<string, decimal> {{"factor", 2}}
            },
        };
        
        private readonly IEnumerable<MarkupPolicy> _counterpartyPolicies = new[]
        {
            new MarkupPolicy
            {
                Id = 4,
                AgentScopeId = $"{MarkupSubject.CounterpartyId}",
                Order = 21,
                Target = MarkupPolicyTarget.AccommodationAvailability,
                AgentScopeType = AgentMarkupScopeTypes.Counterparty,
                TemplateId = 2,
                TemplateSettings = new Dictionary<string, decimal> {{"addition", 32}},
            },
            new MarkupPolicy
            {
                Id = 5,
                AgentScopeId = $"{MarkupSubject.CounterpartyId}",
                Order = 1,
                Target = MarkupPolicyTarget.AccommodationAvailability,
                AgentScopeType = AgentMarkupScopeTypes.Counterparty,
                TemplateId = 1,
                TemplateSettings = new Dictionary<string, decimal> {{"factor", 21}},
            },
        };
        
        private readonly IEnumerable<MarkupPolicy> _globalPolicies = new[]
        {
            new MarkupPolicy
            {
                Id = 7,
                Order = 23,
                Target = MarkupPolicyTarget.AccommodationAvailability,
                AgentScopeType = AgentMarkupScopeTypes.Global,
                TemplateId = 2,
                TemplateSettings = new Dictionary<string, decimal> {{"addition", 14}},
            },
            new MarkupPolicy
            {
                Id = 8,
                Order = 1,
                Target = MarkupPolicyTarget.AccommodationAvailability,
                AgentScopeType = AgentMarkupScopeTypes.Global,
                TemplateId = 1,
                TemplateSettings = new Dictionary<string, decimal> {{"factor", 100}},
            },
        };
        
        private readonly IEnumerable<MarkupPolicy> _agencyPolicies = new[]
        {
            new MarkupPolicy
            {
                Id = 7,
                Order = 1,
                Target = MarkupPolicyTarget.AccommodationAvailability,
                AgentScopeType = AgentMarkupScopeTypes.Agency,
                AgentScopeId = $"{MarkupSubject.AgencyId}",
                TemplateId = 1,
                TemplateSettings = new Dictionary<string, decimal> {{"factor", 100}},
            },
            new MarkupPolicy
            {
                Id = 10,
                Order = 1,
                Target = MarkupPolicyTarget.AccommodationAvailability,
                AgentScopeType = AgentMarkupScopeTypes.Agency,
                AgentScopeId = "1000",
                TemplateId = 2,
                TemplateSettings = new Dictionary<string, decimal> {{"addition", 43}},
            },
            new MarkupPolicy
            {
                Id = 11,
                Order = 1,
                Target = MarkupPolicyTarget.AccommodationAvailability,
                AgentScopeType = AgentMarkupScopeTypes.Agency,
                AgentScopeId = "2000",
                TemplateId = 1,
                TemplateSettings = new Dictionary<string, decimal> {{"factor", 100}},
            },
        };

        private readonly IEnumerable<MarkupPolicy> _agencyLocalityPolicies = new[]
        {
            new MarkupPolicy
            {
                Id = 12,
                Order = 1,
                Target = MarkupPolicyTarget.AccommodationAvailability,
                AgentScopeType = AgentMarkupScopeTypes.Location,
                AgentScopeId = "Locality_01",
                TemplateId = 1,
                TemplateSettings = new Dictionary<string, decimal> { { "factor", 100 } },
            },
            new MarkupPolicy
            {
                Id = 13,
                Order = 2,
                Target = MarkupPolicyTarget.AccommodationAvailability,
                AgentScopeType = AgentMarkupScopeTypes.Location,
                AgentScopeId = "Country_01",
                TemplateId = 2,
                TemplateSettings = new Dictionary<string, decimal> {{"addition", 43}},
            },
        };


        private readonly List<Agency> _agencies = new()
        {
            new Agency
            {
                Id = MarkupSubject.AgencyId,
                Name = "Child agency",
                Ancestors = new List<int> {2000, 1000},
                LocalityHtId = "Locality_01"
            },
            new Agency
            {
                Id = 1000,
                Name = "Parent agency",
                Ancestors = new List<int>{2000},
                LocalityHtId = "Locality_02",
                CountryHtId = "Country_01"
            },
            new Agency
            {
                Id = 2000,
                Name = "Root agency",
                Ancestors = new(),
                LocalityHtId = default
            }
        };

        public void Dispose() { }


        private static readonly MarkupSubjectInfo MarkupSubject = new ()
        {
            AgentId = 1,
            AgencyId = 1,
            CounterpartyId = 1,
            AgencyAncestors = new List<int>{ 2000, 1000 },
            CountryHtId = "Country_01"
        };


        private static readonly MarkupObjectInfo MarkupObject = new()
        {
            CountryHtId = "Country_02",
            LocalityHtId = "Locality_02",
            AccommodationHtId = "Accommodation_01"
        };

        private readonly MarkupPolicyService _markupPolicyService;
        private readonly MarkupService _markupService;
    }
}