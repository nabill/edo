using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Api.Infrastructure.Options;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Accommodations.Availability;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Mapping;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.Edo.UnitTests.Utility;
using HappyTravel.MapperContracts.Public.Accommodations.Internals;
using Image = HappyTravel.MapperContracts.Public.Accommodations.Internals.ImageInfo;
using HappyTravel.MapperContracts.Public.Accommodations;
using HappyTravel.Money.Enums;
using HappyTravel.Money.Models;
using HappyTravel.SupplierOptionsProvider;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using HappyTravel.MapperContracts.Public.Accommodations.Enums;
using System;
using Api.Models.Bookings;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Common.Enums.Administrators;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Agents.AvailabilitySearchTests
{
    public class RecalculatePricesWithCommission
    {
        public RecalculatePricesWithCommission()
        {
            _edoContextMock = MockEdoContextFactory.Create();
            SetupInitialData();

            contractKindCommissionOptions = Options.Create(new ContractKindCommissionOptions
            {
                CreditCardPaymentsCommission = 2m
            });


            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            dateTimeProviderMock.Setup(d => d.UtcNow()).Returns(CancellationDate);

            var bookingRecordManagerMock = new Mock<BookingRecordManager>(_edoContextMock.Object);
            bookingRecordManagerMock
                .Setup(x => x.Get(It.IsAny<string>()))
                .ReturnsAsync(bookings[0]);

            var accommodationMapperClientMock = new Mock<IAccommodationMapperClient>();
            accommodationMapperClientMock
                .Setup(x => x.GetAccommodation(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MakeAccomodation());

            var agentContextServiceMock = new Mock<IAgentContextService>();
            agentContextServiceMock.Setup(x => x.GetAgent())
                .ReturnsAsync(MakeAgentContext(1));
            
            var adminContext = new Mock<IAdministratorContext>();
            adminContext
                .Setup(x => x.HasPermission(It.IsAny<AdministratorPermissions>()))
                .ReturnsAsync(true);
            
            var bookingInfoService = new BookingInfoService(_edoContextMock.Object, bookingRecordManagerMock.Object,
                accommodationMapperClientMock.Object, Mock.Of<IAccommodationBookingSettingsService>(),
                Mock.Of<ISupplierOptionsStorage>(), dateTimeProviderMock.Object, Mock.Of<IAgentContextService>(), adminContext.Object);

            _agentBookingManagementService = new AgentBookingManagementService(_edoContextMock.Object,
                It.IsAny<ISupplierBookingManagementService>(), bookingRecordManagerMock.Object,
                It.IsAny<IBookingStatusRefreshService>(), bookingInfoService, contractKindCommissionOptions,
                agentContextServiceMock.Object);
        }


        [Fact]
        public async Task Room_selection_step_align_prices_should_apply_credit_card_commission_and_return_success()
        {
            var request = new BookingRecalculatePriceRequest(PaymentTypes.CreditCard);

            var (_, IsFailure, booking, error) = await _agentBookingManagementService.RecalculatePrice("any", request, "en");

            Assert.False(IsFailure);
        }


        static AgentContext MakeAgentContext(int agencyId) =>
            new AgentContext(1, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, agencyId,
                string.Empty, true, InAgencyPermissions.All, string.Empty, string.Empty, string.Empty, 2, new List<int>(), ContractKind.CreditCardPayments);


        static Accommodation MakeAccomodation() =>
            new Accommodation(string.Empty, string.Empty, new List<string>(), new Dictionary<string, string>(),
                string.Empty, new ContactInfo(new List<string>(), new List<string>(), new List<string>(), new List<string>()),
                new LocationInfo(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, new GeoPoint(0f, 0f),
                string.Empty, LocationDescriptionCodes.Airport, new List<PoiInfo>(), true), new List<Image>(),
                AccommodationRatings.NotRated, new ScheduleInfo(), new List<TextualDescription>(), PropertyTypes.Hotels, new List<SupplierInfo>(),
                DateTime.Now);


        static BookedRoom MakeBookedRoom(MoneyAmount netPrice) =>
            new(default, default, default, default, default, default, default, default,
                new Deadline(CancellationDate, new List<CancellationPolicy>(), new List<string>(), true),
                default, default, default, default, netPrice);


        private void SetupInitialData()
        {
            _edoContextMock
                .Setup(c => c.Bookings)
                .Returns(DbSetMockProvider.GetDbSetMock(bookings));
            _edoContextMock
                .Setup(c => c.Agents)
                .Returns(DbSetMockProvider.GetDbSetMock(agents));
            _edoContextMock
                .Setup(c => c.AgentAgencyRelations)
                .Returns(DbSetMockProvider.GetDbSetMock(relations));
            _edoContextMock
                .Setup(c => c.Agencies)
                .Returns(DbSetMockProvider.GetDbSetMock(agencies));
        }


        private readonly List<Booking> bookings = new()
        {
            new Booking
            {
                Id = 1,
                NetPrice = 100m,
                Commission = ZeroCommission,
                TotalPrice = 100m,
                Currency = Currencies.USD,
                Rooms = new List<BookedRoom>()
                {
                    MakeBookedRoom(new MoneyAmount(60m, Currencies.USD)),
                    MakeBookedRoom(new MoneyAmount(40m, Currencies.USD))
                },
                AgencyId = 1,
            },
            new Booking
            {
                Id = 2,
                NetPrice = 100m,
                Commission = 2m,
                TotalPrice = 102m,
                Currency = Currencies.USD,
                Rooms = new List<BookedRoom>()
                {
                    MakeBookedRoom(new MoneyAmount(60m, Currencies.USD)),
                    MakeBookedRoom(new MoneyAmount(40m, Currencies.USD))
                },
                AgencyId = 2,
            },
        };

        private readonly List<Agent> agents = new()
        {
            new Agent
            {
                Id = 1,
                IdentityHash = "d04b98f48e8f8bcc15c6ae5ac050801cd6dcfd428fb5f9e65c4e16e7807340fa"
            },
            new Agent
            {
                Id = 2,
                IdentityHash = "d04b98f48e8f8bcc15c6ae5ac050801cd6dcfd428fb5f9e65c4e16e7807340fb"
            }
        };

        private readonly List<Agency> agencies = new()
        {
            new Agency
            {
                Id = 1,
                Name = "Test 1",
                ContractKind = ContractKind.CreditCardPayments,
                CountryCode = "KZ",
                IsActive = true
            },
            new Agency
            {
                Id = 2,
                Name = "Test 2",
                ContractKind = ContractKind.OfflineOrCreditCardPayments,
                CountryCode = "KZ",
                IsActive = true
            },
            new Agency
            {
                Id = 3,
                Name = "Test 3",
                ContractKind = ContractKind.VirtualAccountOrCreditCardPayments,
                CountryCode = "RU",
                IsActive = true
            }
        };

        private readonly List<AgentAgencyRelation> relations = new()
        {
            new AgentAgencyRelation
            {
                AgencyId = 1,
                AgentId = 1,
                IsActive = true
            },
            new AgentAgencyRelation
            {
                AgencyId = 2,
                AgentId = 2,
                IsActive = true
            }
        };

        private const decimal ZeroCommission = 0m;
        private static readonly DateTimeOffset CancellationDate = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);

        private readonly Mock<EdoContext> _edoContextMock;
        private readonly IAgentBookingManagementService _agentBookingManagementService;
        private readonly IOptions<ContractKindCommissionOptions> contractKindCommissionOptions;
    }
}