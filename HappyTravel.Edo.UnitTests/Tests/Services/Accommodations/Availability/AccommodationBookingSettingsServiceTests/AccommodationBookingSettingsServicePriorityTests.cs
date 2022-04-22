using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Accommodations.Availability;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Common.Enums.AgencySettings;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.EdoContracts.General.Enums;
using HappyTravel.SupplierOptionsClient.Models;
using HappyTravel.SupplierOptionsProvider;
using Xunit;
using Moq;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Accommodations.Availability.AccommodationBookingSettingsServiceTests
{
    public class AccommodationBookingSettingsServicePriorityTests
    {
        // Default settings
        [Fact]
        public async Task Defaults_should_apply_when_no_agent_and_agency_settings_found()
        {
            var agentSettings = default(Maybe<AgentAccommodationBookingSettings>);
            var agencySettings = default(Maybe<AgencyAccommodationBookingSettings>);
            var rootAgencySettings = default(RootAgencyAccommodationBookingSettings);
        
            var (agentSettingsService, agencySettingsService, rootAgencySystemSettingsService) = GetSettingsServices(agentSettings, agencySettings, rootAgencySettings);
            var flow = GetDoubleFlow();
            var supplierOptionsStorage = GetSupplierOptionsStorage();
        
            var service = new AccommodationBookingSettingsService(flow, agentSettingsService, agencySettingsService, rootAgencySystemSettingsService,
                supplierOptionsStorage, default);
        
            var settings = await service.Get(_agentContext);
        
            Assert.Equal(DefaultAprMode, settings.AprMode);
            Assert.Equal(DefaultPassedDeadlineOffersMode, settings.PassedDeadlineOffersMode);
            Assert.Equal(default, settings.AdditionalSearchFilters);
            
            var defaultEnabledSuppliers = _suppliers.Where(s => s.IsEnabled)
                .Select(s => s.Code)
                .ToArray();
            
            for (var i = 0; i < defaultEnabledSuppliers.Length; i++)
                Assert.Equal(defaultEnabledSuppliers[i], settings.EnabledConnectors[i]);
        }


        // Settings from one source
        [Fact]
        public async Task RootAgency_setting_must_apply_exactly()
        {
            var agentSettings = default(Maybe<AgentAccommodationBookingSettings>);
            var agencySettings = default(Maybe<AgencyAccommodationBookingSettings>);
            var rootAgencySettings = new RootAgencyAccommodationBookingSettings
            {
                CancellationPolicyProcessSettings = new CancellationPolicyProcessSettings
                {
                    PolicyStartDateShift = new TimeSpan(0, 0, 0, 18)
                }
            };
            var expectedPolicyStartDateShift = new TimeSpan(0, 0, 0, 18);

            var (agentSettingsService, agencySettingsService, rootAgencySystemSettingsService) = GetSettingsServices(agentSettings, agencySettings, rootAgencySettings);
            var flow = GetDoubleFlow();
            var supplierOptionsStorage = GetSupplierOptionsStorage();

            var service = new AccommodationBookingSettingsService(flow, agentSettingsService, agencySettingsService, rootAgencySystemSettingsService,
                supplierOptionsStorage, default);

            var settings = await service.Get(_agentContext);

            Assert.Equal(expectedPolicyStartDateShift, settings.CancellationPolicyProcessSettings.PolicyStartDateShift);
        }


        [Fact]
        public async Task AdditionalSearchFilters_setting_must_apply_exactly_when_agent_settings_found()
        {
            var agentSettings = Maybe<AgentAccommodationBookingSettings>
                .From(new AgentAccommodationBookingSettings{ AdditionalSearchFilters = SearchFilters.BestPrice | SearchFilters.BestRoomPlans });
            var agencySettings = default(Maybe<AgencyAccommodationBookingSettings>);
            var rootAgencySettings = default(RootAgencyAccommodationBookingSettings);

            var (agentSettingsService, agencySettingsService, rootAgencySystemSettingsService) = GetSettingsServices(agentSettings, agencySettings, rootAgencySettings);
            var flow = GetDoubleFlow();
            var supplierOptionsStorage = GetSupplierOptionsStorage();

            var service = new AccommodationBookingSettingsService(flow, agentSettingsService, agencySettingsService, rootAgencySystemSettingsService,
                supplierOptionsStorage, default);

            var settings = await service.Get(_agentContext);

            Assert.Equal(SearchFilters.BestPrice | SearchFilters.BestRoomPlans, settings.AdditionalSearchFilters);
        }


        // Settings taken from one of two sources (agent setting is priority)
        [Fact]
        public async Task Agent_settings_must_apply_when_only_agent_settings_found()
        {
            var expectedSupplierOptions = new List<string> { "supplier1", "supplier2" };
            var agentSettings = Maybe<AgentAccommodationBookingSettings>
                .From(new AgentAccommodationBookingSettings
                {
                    EnabledSuppliers = new List<string> { "supplier1", "supplier2"},
                    AprMode = AprMode.CardPurchasesOnly,
                    PassedDeadlineOffersMode = PassedDeadlineOffersMode.CardAndAccountPurchases
                });
            var agencySettings = default(Maybe<AgencyAccommodationBookingSettings>);
            var rootAgencySettings = default(RootAgencyAccommodationBookingSettings);

            var (agentSettingsService, agencySettingsService, rootAgencySystemSettingsService) = GetSettingsServices(agentSettings, agencySettings, rootAgencySettings);
            var flow = GetDoubleFlow();
            var supplierOptionsStorage = GetSupplierOptionsStorage();

            var service = new AccommodationBookingSettingsService(flow, agentSettingsService, agencySettingsService, rootAgencySystemSettingsService,
                supplierOptionsStorage, default);

            var settings = await service.Get(_agentContext);

            Assert.Equal(AprMode.CardPurchasesOnly, settings.AprMode);
            Assert.Equal(PassedDeadlineOffersMode.CardAndAccountPurchases, settings.PassedDeadlineOffersMode);

            for (int i = 0; i < expectedSupplierOptions.Count; i++)
                Assert.Equal(expectedSupplierOptions[i], settings.EnabledConnectors[i]);
        }


        [Fact]
        public async Task Agent_settings_must_apply_when_agent_and_agency_settings_found()
        {
            var expectedSupplierOptions = new List<string> { "supplier1", "supplier2" };
            var agentSettings = Maybe<AgentAccommodationBookingSettings>
                .From(new AgentAccommodationBookingSettings
                {
                    EnabledSuppliers = new List<string> { "supplier1", "supplier2" },
                    AprMode = AprMode.CardPurchasesOnly,
                    PassedDeadlineOffersMode = PassedDeadlineOffersMode.CardAndAccountPurchases
                });
            var agencySettings = Maybe<AgencyAccommodationBookingSettings>
                .From(new AgencyAccommodationBookingSettings
                {
                    EnabledSuppliers = new List<string> { "supplier3", "supplier4"},
                    AprMode = AprMode.Hide,
                    PassedDeadlineOffersMode = PassedDeadlineOffersMode.Hide
                });
            var rootAgencySettings = default(RootAgencyAccommodationBookingSettings);

            var (agentSettingsService, agencySettingsService, rootAgencySystemSettingsService) = GetSettingsServices(agentSettings, agencySettings, rootAgencySettings);
            var flow = GetDoubleFlow();
            var supplierOptionsStorage = GetSupplierOptionsStorage();

            var service = new AccommodationBookingSettingsService(flow, agentSettingsService, agencySettingsService, rootAgencySystemSettingsService,
                supplierOptionsStorage, default);

            var settings = await service.Get(_agentContext);

            Assert.Equal(AprMode.CardPurchasesOnly, settings.AprMode);
            Assert.Equal(PassedDeadlineOffersMode.CardAndAccountPurchases, settings.PassedDeadlineOffersMode);

            for (int i = 0; i < expectedSupplierOptions.Count; i++)
                Assert.Equal(expectedSupplierOptions[i], settings.EnabledConnectors[i]);
        }


        [Fact]
        public async Task Agency_settings_must_apply_when_only_agency_settings_found()
        {
            var expectedSupplierOptions = new List<string> { "supplier3", "supplier4" };
            var agentSettings = default(Maybe<AgentAccommodationBookingSettings>);
            var agencySettings = Maybe<AgencyAccommodationBookingSettings>
                .From(new AgencyAccommodationBookingSettings
                {
                    EnabledSuppliers = new List<string> { "supplier3", "supplier4" },
                    AprMode = AprMode.Hide,
                    PassedDeadlineOffersMode = PassedDeadlineOffersMode.Hide
                });
            var rootAgencySettings = default(RootAgencyAccommodationBookingSettings);

            var (agentSettingsService, agencySettingsService, rootAgencySystemSettingsService) = GetSettingsServices(agentSettings, agencySettings, rootAgencySettings);
            var flow = GetDoubleFlow();
            var supplierOptionsStorage = GetSupplierOptionsStorage();

            var service = new AccommodationBookingSettingsService(flow, agentSettingsService, agencySettingsService, rootAgencySystemSettingsService,
                supplierOptionsStorage, default);

            var settings = await service.Get(_agentContext);

            Assert.Equal(AprMode.Hide, settings.AprMode);
            Assert.Equal(PassedDeadlineOffersMode.Hide, settings.PassedDeadlineOffersMode);

            for (int i = 0; i < expectedSupplierOptions.Count; i++)
                Assert.Equal(expectedSupplierOptions[i], settings.EnabledConnectors[i]);
        }


        private IDoubleFlow GetDoubleFlow()
        {
            var mock = new Mock<IDoubleFlow>();
            mock.Setup(m => m.GetOrSetAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<Task<AccommodationBookingSettings>>>(),
                    It.IsAny<TimeSpan>(),
                    default))
                .Returns(
                    (string _, Func<Task<AccommodationBookingSettings>> b, TimeSpan _, CancellationToken _) => b());

            mock.Setup(m => m.Options)
                .Returns(new FlowOptions {CacheKeyDelimiter = ", "});

            return mock.Object;
        }

        private (IAgentSystemSettingsService, IAgencySystemSettingsService, IRootAgencySystemSettingsService) GetSettingsServices(
            Maybe<AgentAccommodationBookingSettings> agentSettings,
            Maybe<AgencyAccommodationBookingSettings> agencySettings,
            RootAgencyAccommodationBookingSettings rootAgencySettings)
        {
            var agentMock = new Mock<IAgentSystemSettingsService>();
            agentMock.Setup(m => m.GetAccommodationBookingSettings(It.IsAny<AgentContext>()))
                .Returns(() => Task.FromResult(agentSettings));
            
            var agencyMock = new Mock<IAgencySystemSettingsService>();
            agencyMock.Setup(m => m.GetAccommodationBookingSettings(It.IsAny<int>()))
                .Returns(() => Task.FromResult(agencySettings));
            
            var rootAgencySettingsMock = new Mock<IRootAgencySystemSettingsService>();
            rootAgencySettingsMock.Setup(m => m.GetAccommodationBookingSettings(It.IsAny<int>()))
                .Returns(() => Task.FromResult(rootAgencySettings));

            return (agentMock.Object, agencyMock.Object, rootAgencySettingsMock.Object);
        }


        private ISupplierOptionsStorage GetSupplierOptionsStorage()
        {
            var mock = new Mock<ISupplierOptionsStorage>();
            mock.Setup(m => m.GetAll())
                .Returns(_suppliers);
            return mock.Object;
        }


        private readonly AgentContext _agentContext = new AgentContext(1, "fn", "ln", "email", "title", "pos", 1, "aname", default, default, "", "", 1, new());
        private readonly List<SlimSupplier> _suppliers = new()
        {
            new SlimSupplier
            {
                Code = "Supplier1",
                IsEnabled = true
            },
            new SlimSupplier
            {
                Code = "Supplier2",
                IsEnabled = false
            },
            new SlimSupplier
            {
                Code = "Supplier3",
                IsEnabled = true
            }
        };


        private const PassedDeadlineOffersMode DefaultPassedDeadlineOffersMode = PassedDeadlineOffersMode.DisplayOnly;
        private const AprMode DefaultAprMode = AprMode.DisplayOnly;
    }
}
