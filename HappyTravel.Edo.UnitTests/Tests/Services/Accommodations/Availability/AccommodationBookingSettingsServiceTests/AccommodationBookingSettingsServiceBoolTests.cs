using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Accommodations.Availability;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Data.Agents;
using Xunit;
using Moq;
using Api.AdministratorServices;
using HappyTravel.Edo.Common.Enums.AgencySettings;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Accommodations.Availability.AccommodationBookingSettingsServiceTests
{
    public class AccommodationBookingSettingsServiceBoolTests
    {
        // Settings combined from two sources
        [Fact]
        public async Task Combined_bool_settings_must_be_false_if_no_source_found()
        {
            var agentSettings = DefaultAgentMaterializedSettings;
            var agencySettings = DefaultAgencyMaterializedSettings;
            var rootAgencySettings = DefaultRootMaterializedSettings;
            var agentContextService = Mock.Of<IAgentContextService>();

            var (agentSettingsService, agencySettingsService, rootAgencySystemSettingsService) = GetSettingsServices(agentSettings, agencySettings, rootAgencySettings);
            var flow = GetDoubleFlow();
            var agentSupplierManagementService = GetAgentSupplierManagementService();

            var service = new AccommodationBookingSettingsService(flow, agentSettingsService, agencySettingsService, rootAgencySystemSettingsService,
                agentSupplierManagementService, default, agentContextService);

            var settings = await service.Get();

            Assert.False(settings.IsSupplierVisible);
            Assert.False(settings.IsDirectContractFlagVisible);
        }


        [Theory]
        [InlineData(false, false, false)]
        [InlineData(false, false, true)]
        [InlineData(false, true, false)]
        [InlineData(false, true, true)]
        [InlineData(true, false, false)]
        [InlineData(true, false, true)]
        [InlineData(true, true, false)]
        [InlineData(true, true, true)]
        public async Task Agent_combined_bool_settings_must_apply_when_only_agent_settings_found(
            bool expectedMarkupDisabled,
            bool expectedSupplierVisible,
            bool expectedDirectContractFlagVisible)
        {
            var agentSettings = DefaultAgentMaterializedSettings;
            agentSettings.IsSupplierVisible = expectedSupplierVisible;
            agentSettings.IsDirectContractFlagVisible = expectedDirectContractFlagVisible;

            var agencySettings = DefaultAgencyMaterializedSettings;
            var rootAgencySettings = DefaultRootMaterializedSettings;
            var agentContextService = Mock.Of<IAgentContextService>();

            var (agentSettingsService, agencySettingsService, rootAgencySystemSettingsService) = GetSettingsServices(agentSettings, agencySettings, rootAgencySettings);
            var flow = GetDoubleFlow();
            var agencySupplierManagementService = GetAgentSupplierManagementService();

            var service = new AccommodationBookingSettingsService(flow, agentSettingsService, agencySettingsService, rootAgencySystemSettingsService,
                agencySupplierManagementService, default, agentContextService);

            var settings = await service.Get();

            Assert.Equal(expectedSupplierVisible, settings.IsSupplierVisible);
            Assert.Equal(expectedDirectContractFlagVisible, settings.IsDirectContractFlagVisible);
        }


        [Theory]
        [InlineData(false, false, false)]
        [InlineData(false, false, true)]
        [InlineData(false, true, false)]
        [InlineData(false, true, true)]
        [InlineData(true, false, false)]
        [InlineData(true, false, true)]
        [InlineData(true, true, false)]
        [InlineData(true, true, true)]
        public async Task Agency_combined_bool_settings_must_apply_when_only_agency_settings_found(
            bool expectedMarkupDisabled,
            bool expectedSupplierVisible,
            bool expectedDirectContractFlagVisible)
        {
            var agentSettings = DefaultAgentMaterializedSettings;
            agentSettings.IsSupplierVisible = expectedSupplierVisible;
            agentSettings.IsDirectContractFlagVisible = expectedDirectContractFlagVisible;
            var agencySettings = DefaultAgencyMaterializedSettings;
            agencySettings.IsSupplierVisible = expectedSupplierVisible;
            agencySettings.IsDirectContractFlagVisible = expectedDirectContractFlagVisible;

            var rootAgencySettings = DefaultRootMaterializedSettings;
            var agentContextService = Mock.Of<IAgentContextService>();

            var (agentSettingsService, agencySettingsService, rootAgencySystemSettingsService) = GetSettingsServices(agentSettings, agencySettings, rootAgencySettings);
            var flow = GetDoubleFlow();
            var agencySupplierManagementService = GetAgentSupplierManagementService();

            var service = new AccommodationBookingSettingsService(flow, agentSettingsService, agencySettingsService, rootAgencySystemSettingsService,
                agencySupplierManagementService, default, agentContextService);

            var settings = await service.Get();

            Assert.Equal(expectedSupplierVisible, settings.IsSupplierVisible);
            Assert.Equal(expectedDirectContractFlagVisible, settings.IsDirectContractFlagVisible);
        }


        [Theory]
        [InlineData(false, false, false, true, true, true, false, false, true)]
        [InlineData(true, false, true, false, false, false, true, true, false)]
        [InlineData(true, true, false, false, false, true, true, true, true)]
        [InlineData(true, true, true, true, true, false, false, false, false)]
        public async Task Combined_bool_settings_must_be_true_if_any_source_gives_true(
            bool expectedMarkupDisabled, bool agentMarkupDisabled, bool agencyMarkupDisabled,
            bool expectedSupplierVisible, bool agentSupplierVisible, bool agencySupplierVisible,
            bool expectedDirectContractFlagVisible, bool agentDirectContractFlagVisible, bool agencyDirectContractFlagVisible)
        {
            var agentSettings = DefaultAgentMaterializedSettings;
            agentSettings.IsSupplierVisible = agentSupplierVisible;
            agentSettings.IsDirectContractFlagVisible = agentDirectContractFlagVisible;

            var agencySettings = DefaultAgencyMaterializedSettings;
            agencySettings.IsSupplierVisible = agencySupplierVisible;
            agencySettings.IsDirectContractFlagVisible = agencyDirectContractFlagVisible;

            var rootAgencySettings = DefaultRootMaterializedSettings;
            var agentContextService = Mock.Of<IAgentContextService>();

            var (agentSettingsService, agencySettingsService, rootAgencySystemSettingsService) = GetSettingsServices(agentSettings, agencySettings, rootAgencySettings);
            var flow = GetDoubleFlow();
            var agencySupplierManagementService = GetAgentSupplierManagementService();

            var service = new AccommodationBookingSettingsService(flow, agentSettingsService, agencySettingsService, rootAgencySystemSettingsService,
                agencySupplierManagementService, default, agentContextService);

            var settings = await service.Get();

            Assert.Equal(expectedSupplierVisible, settings.IsSupplierVisible);
            Assert.Equal(expectedDirectContractFlagVisible, settings.IsDirectContractFlagVisible);
        }


        private AgentAccommodationBookingSettings DefaultAgentMaterializedSettings
            => new AgentAccommodationBookingSettings
            {
                AdditionalSearchFilters = new(),
                AprMode = AprMode.Hide,
                CustomDeadlineShift = 0,
                IsDirectContractFlagVisible = false,
                IsSupplierVisible = false,
                PassedDeadlineOffersMode = PassedDeadlineOffersMode.Hide
            };


        private AgencyAccommodationBookingSettings DefaultAgencyMaterializedSettings
            => new AgencyAccommodationBookingSettings()
            {
                IsSupplierVisible = false,
                IsDirectContractFlagVisible = false,
                AprMode = AprMode.Hide,
                PassedDeadlineOffersMode = PassedDeadlineOffersMode.Hide,
                CustomDeadlineShift = 0
            };


        private RootAgencyAccommodationBookingSettings DefaultRootMaterializedSettings
            => new RootAgencyAccommodationBookingSettings()
            {
                CancellationPolicyProcessSettings = new CancellationPolicyProcessSettings
                {
                    PolicyStartDateShift = TimeSpan.FromDays(-3)
                }
            };


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
                .Returns(new FlowOptions { CacheKeyDelimiter = ", " });

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


        private IAgentSupplierManagementService GetAgentSupplierManagementService()
        {
            var mock = new Mock<IAgentSupplierManagementService>();
            mock.Setup(m => m.GetMaterializedSuppliers(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.FromResult(Result.Success(new Dictionary<string, bool>()
                {
                    { "netstorming", true },
                    { "illusions", true },
                    { "etg", false }
                })));
            return mock.Object;
        }


        private readonly AgentContext _agentContext =
            new AgentContext(1, "fn", "ln", "email", "title", "pos", 1, "aName",
                default, default, string.Empty, string.Empty, string.Empty, 1, new(), ContractKind.OfflineOrCreditCardPayments);
    }
}
