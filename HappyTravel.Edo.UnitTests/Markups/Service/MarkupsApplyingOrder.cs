using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.CurrencyConversion;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Markups;
using HappyTravel.Edo.Api.Services.Markups.Templates;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Markup;
using HappyTravel.Edo.UnitTests.Infrastructure;
using HappyTravel.Edo.UnitTests.Infrastructure.DbSetMocks;
using HappyTravel.Money.Enums;
using Moq;
using NetTopologySuite.Utilities;
using Xunit;
using Assert = Xunit.Assert;

namespace HappyTravel.Edo.UnitTests.Markups.Service
{
    public class MarkupsApplyingOrder
    {
        public MarkupsApplyingOrder(Mock<EdoContext> edoContextMock, IMemoryFlow memoryFlow)
        {
            var allPolicies = _agentPolicies
                .Union(_counterpartyPolicies)
                .Union(_globalPolicies)
                .Union(_agencyPolicies);
            
            edoContextMock.Setup(c => c.MarkupPolicies)
                .Returns(DbSetMockProvider.GetDbSetMock(allPolicies));
            
            var currencyRateServiceMock = new Mock<ICurrencyRateService>();
            currencyRateServiceMock
                .Setup(c => c.Get(It.IsAny<Currencies>(), It.IsAny<Currencies>()))
                .Returns(new ValueTask<Result<decimal>>(Result.Ok((decimal)1)));

            var agentSettingsMock = new Mock<IAgentSettingsManager>();
            
            agentSettingsMock
                .Setup(s => s.GetUserSettings(It.IsAny<AgentInfo>()))
                .Returns(Task.FromResult(new AgentUserSettings(true, It.IsAny<Currencies>(), It.IsAny<Currencies>())));
                
            _markupService = new MarkupService(edoContextMock.Object,
                memoryFlow,
                new MarkupPolicyTemplateService(),
                currencyRateServiceMock.Object,
                agentSettingsMock.Object);
        }


        [Fact]
        public async Task Policies_should_be_ordered_by_scope()
        {
            var markup = await _markupService.Get(AgentInfo, MarkupPolicyTarget.AccommodationAvailability);
            var policies = markup.Policies;
            for (var i = 0; i < policies.Count - 1; i++)
            {
                Assert.True(ScopeOrderIsCorrect(policies[i].ScopeType, policies[i + 1].ScopeType));
            }

            bool ScopeOrderIsCorrect(MarkupPolicyScopeType firstScope, MarkupPolicyScopeType secondScope)
            {
                switch (firstScope)
                {
                    case MarkupPolicyScopeType.Global:
                    {
                        return true;
                    }
                    case MarkupPolicyScopeType.Counterparty:
                    {
                        return secondScope != MarkupPolicyScopeType.Global;
                    }
                    case MarkupPolicyScopeType.Agency:
                    {
                        return secondScope != MarkupPolicyScopeType.Global &&
                            secondScope != MarkupPolicyScopeType.Counterparty;
                    }
                    case MarkupPolicyScopeType.Agent:
                    {
                        return secondScope != MarkupPolicyScopeType.Global &&
                            secondScope != MarkupPolicyScopeType.Counterparty &&
                            secondScope != MarkupPolicyScopeType.Agency;
                    }
                    case MarkupPolicyScopeType.EndClient:
                    {
                        return secondScope != MarkupPolicyScopeType.Global &&
                            secondScope != MarkupPolicyScopeType.Counterparty &&
                            secondScope != MarkupPolicyScopeType.Agency &&
                            secondScope != MarkupPolicyScopeType.Agent;
                    }
                    default: throw new AssertionFailedException("Unexpected scope type");
                }
            }
        }
        
        [Fact]
        public async Task Policies_in_scope_should_be_ordered_by_order()
        {
            var markup = await _markupService.Get(AgentInfo, MarkupPolicyTarget.AccommodationAvailability);
            var policies = markup.Policies;
            for (var i = 0; i < policies.Count - 1; i++)
            {
                Assert.True(ScopeOrderIsCorrect(policies[i], policies[i + 1]));
            }

            bool ScopeOrderIsCorrect(MarkupPolicy firstPolicy, MarkupPolicy secondPolicy)
            {
                if (firstPolicy.ScopeType != secondPolicy.ScopeType)
                    return true;

                return firstPolicy.Order < secondPolicy.Order;
            }
        }

        [Theory]
        [InlineData(100, Currencies.EUR, 42065202)]
        [InlineData(240.5, Currencies.USD, 101075202.0)]
        [InlineData(0.13, Currencies.USD, 119802.00)]
        public async Task Policies_calculation_should_execute_in_right_order(decimal supplierPrice, Currencies currency, decimal expectedResultPrice)
        {
            var markup = await _markupService.Get(AgentInfo, MarkupPolicyTarget.AccommodationAvailability);
            var (resultPrice, _) = await markup.Function(supplierPrice, currency);
            Assert.Equal(expectedResultPrice, resultPrice);
        }

        private readonly IEnumerable<MarkupPolicy> _agentPolicies = new[]
        {
            new MarkupPolicy
            {
                Id = 1,
                AgentId = AgentInfo.AgentId,
                Order = 21,
                Target = MarkupPolicyTarget.AccommodationAvailability,
                ScopeType = MarkupPolicyScopeType.Agent,
                TemplateId = 2,
                TemplateSettings = new Dictionary<string, decimal> {{"addition", 2}},
            },
            new MarkupPolicy
            {
                Id = 2,
                AgentId = AgentInfo.AgentId,
                Order = 1,
                Target = MarkupPolicyTarget.AccommodationAvailability,
                ScopeType = MarkupPolicyScopeType.Agent,
                TemplateId = 1,
                TemplateSettings = new Dictionary<string, decimal> {{"factor", 2}},
            }
        };
        
        private readonly IEnumerable<MarkupPolicy> _counterpartyPolicies = new[]
        {
            new MarkupPolicy
            {
                Id = 3,
                CounterpartyId = AgentInfo.CounterpartyId,
                Order = 21,
                Target = MarkupPolicyTarget.AccommodationAvailability,
                ScopeType = MarkupPolicyScopeType.Counterparty,
                TemplateId = 2,
                TemplateSettings = new Dictionary<string, decimal> {{"addition", 32}},
            },
            new MarkupPolicy
            {
                Id = 4,
                CounterpartyId = AgentInfo.CounterpartyId,
                Order = 1,
                Target = MarkupPolicyTarget.AccommodationAvailability,
                ScopeType = MarkupPolicyScopeType.Counterparty,
                TemplateId = 1,
                TemplateSettings = new Dictionary<string, decimal> {{"factor", 21}},
            },
        };
        
        private readonly IEnumerable<MarkupPolicy> _globalPolicies = new[]
        {
            new MarkupPolicy
            {
                Id = 5,
                Order = 23,
                Target = MarkupPolicyTarget.AccommodationAvailability,
                ScopeType = MarkupPolicyScopeType.Global,
                TemplateId = 2,
                TemplateSettings = new Dictionary<string, decimal> {{"addition", 14}},
            },
            new MarkupPolicy
            {
                Id = 6,
                Order = 1,
                Target = MarkupPolicyTarget.AccommodationAvailability,
                ScopeType = MarkupPolicyScopeType.Global,
                TemplateId = 1,
                TemplateSettings = new Dictionary<string, decimal> {{"factor", 100}},
            }
        };
        
        private readonly IEnumerable<MarkupPolicy> _agencyPolicies = new[]
        {
            new MarkupPolicy
            {
                Id = 7,
                Order = 1,
                AgencyId = AgentInfo.AgencyId,
                Target = MarkupPolicyTarget.AccommodationAvailability,
                ScopeType = MarkupPolicyScopeType.Agency,
                TemplateId = 1,
                TemplateSettings = new Dictionary<string, decimal> {{"factor", 100}},
            }
        };
        
        private IEnumerable<MarkupPolicy> _endClientPolicies = new[]
        {
            new MarkupPolicy
            {
                Id = 8,
                Order = 111,
                AgentId = AgentInfo.AgentId,
                Target = MarkupPolicyTarget.AccommodationAvailability,
                ScopeType = MarkupPolicyScopeType.EndClient,
                TemplateId = 1,
                TemplateSettings = new Dictionary<string, decimal> {{"factor", 100}},
            },
            new MarkupPolicy
            {
                Id = 9,
                Order = 14,
                AgentId = AgentInfo.AgentId,
                Target = MarkupPolicyTarget.AccommodationAvailability,
                ScopeType = MarkupPolicyScopeType.EndClient,
                TemplateId = 1,
                TemplateSettings = new Dictionary<string, decimal> {{"factor", 14}},
            }
        };
        
        private static readonly AgentInfo AgentInfo = AgentInfoFactory.CreateByWithCounterpartyAndAgency(1, 1, 1);
        private readonly MarkupService _markupService;
    }
}