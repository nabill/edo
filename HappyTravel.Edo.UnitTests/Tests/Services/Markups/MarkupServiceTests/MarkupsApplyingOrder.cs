using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Options;
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
                .Union(_globalPolicies)
                .Union(_agencyPolicies)
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

            _markupService = new MarkupService(_markupPolicyService, new MarkupPolicyTemplateService(), currencyRateServiceMock.Object, new FakeMemoryFlow());
        }
    
    
        [Fact]
        public void Policies_should_be_ordered_by_scope()
        {
            var policies = _markupPolicyService.Get(MarkupSubject, default);
            for (var i = 0; i < policies.Count - 1; i++)
            {
                Assert.True(ScopeOrderIsCorrect(policies[i].SubjectScopeType, policies[i + 1].SubjectScopeType));
            }
    
            bool ScopeOrderIsCorrect(SubjectMarkupScopeTypes firstScope, SubjectMarkupScopeTypes secondScope)
            {
                return firstScope switch
                {
                    SubjectMarkupScopeTypes.Global => true,
                    SubjectMarkupScopeTypes.Country => secondScope is not SubjectMarkupScopeTypes.Global,
                    SubjectMarkupScopeTypes.Locality => secondScope is not SubjectMarkupScopeTypes.Global 
                        and not SubjectMarkupScopeTypes.Country,
                    SubjectMarkupScopeTypes.Agency => secondScope is not SubjectMarkupScopeTypes.Global 
                        and not SubjectMarkupScopeTypes.Country and not SubjectMarkupScopeTypes.Locality,
                    SubjectMarkupScopeTypes.Agent => secondScope is not SubjectMarkupScopeTypes.Global 
                        and not SubjectMarkupScopeTypes.Country and not SubjectMarkupScopeTypes.Locality 
                        and not SubjectMarkupScopeTypes.Agency,
                    _ => throw new AssertionFailedException("Unexpected scope type")
                };
            }
        }
        

        [Fact]
        public void Agencies_policies_should_be_ordered_by_agency_tree()
        {
            var policies = _markupPolicyService.Get(MarkupSubject, default);
            var agencyPolicies = policies.Where(p => p.SubjectScopeType == SubjectMarkupScopeTypes.Agency).ToList();
            
            for (var i = 0; i < agencyPolicies.Count - 1; i++)
            {
                Assert.True(int.Parse(agencyPolicies[i].SubjectScopeId) == MarkupSubject.AgencyAncestors[i]);
            }
        }
    
        
        private readonly IEnumerable<MarkupPolicy> _agentPolicies = new[]
        {
            new MarkupPolicy
            {
                Id = 1,
                SubjectScopeType = SubjectMarkupScopeTypes.Agent,
                SubjectScopeId = $"{MarkupSubject.AgencyId}-{MarkupSubject.AgentId}",
                Value = 100,
                FunctionType = MarkupFunctionType.Percent
            },
            new MarkupPolicy
            {
                Id = 2,
                SubjectScopeId = $"{MarkupSubject.AgencyId}-{MarkupSubject.AgentId}",
                SubjectScopeType = SubjectMarkupScopeTypes.Agent,
                FunctionType = MarkupFunctionType.Percent,
                Value = 100
            },
        };
        
        private readonly IEnumerable<MarkupPolicy> _globalPolicies = new[]
        {
            new MarkupPolicy
            {
                Id = 7,
                SubjectScopeType = SubjectMarkupScopeTypes.Global,
                FunctionType = MarkupFunctionType.Percent,
                Value = 1300
            },
            new MarkupPolicy
            {
                Id = 8,
                SubjectScopeType = SubjectMarkupScopeTypes.Global,
                FunctionType = MarkupFunctionType.Percent,
                Value = 900,
            },
        };
        
        private readonly IEnumerable<MarkupPolicy> _agencyPolicies = new[]
        {
            new MarkupPolicy
            {
                Id = 7,
                SubjectScopeType = SubjectMarkupScopeTypes.Agency,
                SubjectScopeId = $"{MarkupSubject.AgencyId}",
                FunctionType = MarkupFunctionType.Percent,
                Value = 900 
            },
            new MarkupPolicy
            {
                Id = 10,
                SubjectScopeType = SubjectMarkupScopeTypes.Agency,
                SubjectScopeId = "1000",
                FunctionType = MarkupFunctionType.Percent,
                Value = 4200
            },
            new MarkupPolicy
            {
                Id = 11,
                SubjectScopeType = SubjectMarkupScopeTypes.Agency,
                SubjectScopeId = "2000",
                FunctionType = MarkupFunctionType.Percent,
                Value = 900
            }
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
            AgencyAncestors = new List<int>{ 2000, 1000 },
            CountryHtId = "Country_01"
        };

        
        private readonly MarkupPolicyService _markupPolicyService;
        private readonly MarkupService _markupService;
    }
}