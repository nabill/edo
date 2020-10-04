using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.CurrencyConversion;
using HappyTravel.Edo.Api.Services.Markups;
using HappyTravel.Edo.Api.Services.Markups.Templates;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Edo.Data.Markup;
using HappyTravel.Edo.UnitTests.Mocks;
using HappyTravel.Edo.UnitTests.Utility;
using HappyTravel.Money.Enums;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Markups.MarkupServiceTests
{
    public class MarkupDisable
    {
        [Theory]
        [InlineData(100, Currencies.EUR, 14000000)]
        [InlineData(240.5, Currencies.USD, 33670000)]
        [InlineData(0.13, Currencies.USD, 18200)]
        public async Task Markups_should_be_applied_if_enabled(decimal supplierPrice, Currencies currency, decimal expectedResultPrice)
        {
            var agencySystemSettingsMockWithEnabledMarkups = new Mock<IAgencySystemSettingsService>();
            agencySystemSettingsMockWithEnabledMarkups
                .Setup(s => s.GetAvailabilitySearchSettings(AgentContext.AgencyId))
                .ReturnsAsync( new AgencyAvailabilitySearchSettings
                {
                    IsMarkupDisabled = false
                });
            var markupService = CreateMarkupService(agencySystemSettingsMockWithEnabledMarkups.Object);
            
            var markup = await markupService.Get(AgentContext, MarkupPolicyTarget.AccommodationAvailability);
            
            var (resultPrice, _) = await markup.Function(supplierPrice, currency);
            Assert.Equal(expectedResultPrice, resultPrice);
        }
        
        
        [Theory]
        [InlineData(100, Currencies.EUR, 100)]
        [InlineData(240.5, Currencies.USD, 240.5)]
        [InlineData(0.13, Currencies.USD, 0.13)]
        public async Task Markups_should_not_be_applied_if_disabled(decimal supplierPrice, Currencies currency, decimal expectedResultPrice)
        {
            var agencySystemSettingsMockWithEnabledMarkups = new Mock<IAgencySystemSettingsService>();
            agencySystemSettingsMockWithEnabledMarkups
                .Setup(s => s.GetAvailabilitySearchSettings(AgentContext.AgencyId))
                .ReturnsAsync( new AgencyAvailabilitySearchSettings
                {
                    IsMarkupDisabled = true
                });
            var markupService = CreateMarkupService(agencySystemSettingsMockWithEnabledMarkups.Object);
            
            var markup = await markupService.Get(AgentContext, MarkupPolicyTarget.AccommodationAvailability);
            
            var (resultPrice, _) = await markup.Function(supplierPrice, currency);
            Assert.Equal(expectedResultPrice, resultPrice);
        }

        
        private IMarkupService CreateMarkupService(IAgencySystemSettingsService agencySystemSettingsService)
        {
            var edoContextMock = MockEdoContextFactory.Create();
            var flow = new FakeDoubleFlow();
            
            edoContextMock.Setup(c => c.MarkupPolicies)
                .Returns(DbSetMockProvider.GetDbSetMock(_policies));
            
            var currencyRateServiceMock = new Mock<ICurrencyRateService>();
            currencyRateServiceMock
                .Setup(c => c.Get(It.IsAny<Currencies>(), It.IsAny<Currencies>()))
                .Returns(new ValueTask<Result<decimal>>(Result.Success((decimal)1)));

            var agentSettingsMock = new Mock<IAgentSettingsManager>();
            
            agentSettingsMock
                .Setup(s => s.GetUserSettings(It.IsAny<AgentContext>()))
                .Returns(Task.FromResult(new AgentUserSettings(true, It.IsAny<Currencies>(), It.IsAny<Currencies>(), It.IsAny<int>())));
            
            return new MarkupService(edoContextMock.Object,
                flow,
                new MarkupPolicyTemplateService(),
                currencyRateServiceMock.Object,
                agentSettingsMock.Object,
                agencySystemSettingsService);
        }
        
        private readonly IEnumerable<MarkupPolicy> _policies = new[]
        {
            new MarkupPolicy
            {
                Id = 6,
                Order = 1,
                Target = MarkupPolicyTarget.AccommodationAvailability,
                ScopeType = MarkupPolicyScopeType.Global,
                TemplateId = 1,
                TemplateSettings = new Dictionary<string, decimal> {{"factor", 100}},
            },
            new MarkupPolicy
            {
                Id = 7,
                Order = 1,
                AgencyId = AgentContext.AgencyId,
                Target = MarkupPolicyTarget.AccommodationAvailability,
                ScopeType = MarkupPolicyScopeType.Agency,
                TemplateId = 1,
                TemplateSettings = new Dictionary<string, decimal> {{"factor", 100}},
            },
            new MarkupPolicy
            {
                Id = 9,
                Order = 14,
                AgentId = AgentContext.AgentId,
                Target = MarkupPolicyTarget.AccommodationAvailability,
                ScopeType = MarkupPolicyScopeType.EndClient,
                TemplateId = 1,
                TemplateSettings = new Dictionary<string, decimal> {{"factor", 14}},
            }
        };
        
        
        private static readonly AgentContext AgentContext = AgentInfoFactory.CreateByWithCounterpartyAndAgency(1, 1, 1);
    }
}