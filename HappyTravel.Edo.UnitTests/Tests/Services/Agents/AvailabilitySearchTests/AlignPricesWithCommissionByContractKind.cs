using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Edo.Data.Locations;
using HappyTravel.Edo.UnitTests.Mocks;
using HappyTravel.Edo.UnitTests.Utility;
using Microsoft.AspNetCore.Http;
using CSharpFunctionalExtensions;
using Moq;
using Xunit;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.RoomSelection;
using HappyTravel.Edo.Api.Services.Accommodations.Availability;
using Microsoft.Extensions.Options;
using Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Accommodations;
using System;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.EdoContracts.Accommodations.Enums;
using HappyTravel.Money.Models;
using HappyTravel.Money.Enums;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.BookingEvaluation;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch;
using HappyTravel.Edo.Api.Models.Agents;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Agents.AvailabilitySearchTests
{
    public class AlignPricesWithCommissionByContractKind
    {
        public AlignPricesWithCommissionByContractKind()
        {
            _edoContextMock = MockEdoContextFactory.Create();
            SetupInitialData();

            contractKindCommissionOptions = Options.Create(new ContractKindCommissionOptions
            {
                CreditCardPaymentsCommission = 2m
            });

            _roomSelectionPriceProcessor = new RoomSelectionPriceProcessor(It.IsAny<IPriceProcessor>(), _edoContextMock.Object,
                contractKindCommissionOptions);
            _bookingEvaluationPriceProcessor = new BookingEvaluationPriceProcessor(It.IsAny<IPriceProcessor>(), _edoContextMock.Object,
                contractKindCommissionOptions);
            _wideAvailabilityPriceProcessor = new WideAvailabilityPriceProcessor(It.IsAny<IPriceProcessor>(), _edoContextMock.Object,
                contractKindCommissionOptions);
        }


        [Fact]
        public async Task Room_selection_step_align_prices_should_apply_credit_card_commission_and_return_success()
        {
            var availabilityDetails = new SingleAccommodationAvailability("id", DateTimeOffset.Now, roomContractSets, "htId",
                "countryHtId", "localityHtId", 2, "KZ", "jumeirah");
            var agencyId = 1;
            var agentContext = new AgentContext(1, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, agencyId,
                string.Empty, true, InAgencyPermissions.All, string.Empty, string.Empty, string.Empty, 2, new List<int>());

            var result = await _roomSelectionPriceProcessor.AlignPrices(availabilityDetails, agentContext);

            Assert.All(result.RoomContractSets, r
                => Assert.Equal(contractKindCommissionOptions.Value.CreditCardPaymentsCommission, r.Rate.Commission));
            Assert.All(result.RoomContractSets, r
                => Assert.True(CommissionApplied(r.Rate.NetPrice.Amount, r.Rate.FinalPrice.Amount, r.Rate.Commission)));
            Assert.All(result.RoomContractSets, s
                => Assert.All(s.Rooms, r
                    => Assert.Equal(contractKindCommissionOptions.Value.CreditCardPaymentsCommission, r.Rate.Commission)));
            Assert.All(result.RoomContractSets, s
                => Assert.All(s.Rooms, r
                    => Assert.True(CommissionApplied(r.Rate.NetPrice.Amount, r.Rate.FinalPrice.Amount, r.Rate.Commission))));
        }


        [Fact]
        public async Task Booking_step_align_prices_should_apply_credit_card_commission_and_return_success()
        {
            var availabilityDetails = new RoomContractSetAvailability("id", DateTimeOffset.Now, DateTimeOffset.Now, 7,
                new SlimAccommodation(), roomContractSets[0], new List<PaymentTypes>(), "countryHtId", "localityHtId",
                "evaluationToken", 2, "KZ", "jumeirah");
            var agencyId = 1;
            var agentContext = new AgentContext(1, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, agencyId,
                string.Empty, true, InAgencyPermissions.All, string.Empty, string.Empty, string.Empty, 2, new List<int>());

            var result = await _bookingEvaluationPriceProcessor.AlignPrices(availabilityDetails, agentContext);

            Assert.Equal(contractKindCommissionOptions.Value.CreditCardPaymentsCommission, result.RoomContractSet.Rate.Commission);
            Assert.True(CommissionApplied(result.RoomContractSet.Rate.NetPrice.Amount,
                result.RoomContractSet.Rate.FinalPrice.Amount, result.RoomContractSet.Rate.Commission));
            Assert.All(result.RoomContractSet.Rooms, r
                    => Assert.Equal(contractKindCommissionOptions.Value.CreditCardPaymentsCommission, r.Rate.Commission));
            Assert.All(result.RoomContractSet.Rooms, r
                    => Assert.True(CommissionApplied(r.Rate.NetPrice.Amount, r.Rate.FinalPrice.Amount, r.Rate.Commission)));
        }


        [Fact]
        public async Task Wide_availability_step_align_prices_should_apply_credit_card_commission_and_return_success()
        {
            var availabilityDetails = new List<AccommodationAvailabilityResult>()
            {
                new AccommodationAvailabilityResult(Guid.NewGuid(),
                    "jumeirah", DateTimeOffset.Now, "id", roomContractSets, 0m,
                    Decimal.MaxValue, DateTimeOffset.Now, DateTimeOffset.Now, "htId", "supplierAccommodationCode",
                    "countryHtId", "localityHtId", 2, "KZ")
            };
            var agencyId = 1;
            var agentContext = new AgentContext(1, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, agencyId,
                string.Empty, true, InAgencyPermissions.All, string.Empty, string.Empty, string.Empty, 2, new List<int>());

            var result = await _wideAvailabilityPriceProcessor.AlignPrices(availabilityDetails, agentContext);

            Assert.All(result, a
                => Assert.All(a.RoomContractSets, r
                    => Assert.Equal(contractKindCommissionOptions.Value.CreditCardPaymentsCommission, r.Rate.Commission)));
            Assert.All(result, a
                => Assert.All(a.RoomContractSets, r
                    => Assert.True(CommissionApplied(r.Rate.NetPrice.Amount, r.Rate.FinalPrice.Amount, r.Rate.Commission))));
            Assert.All(result, a
                => Assert.All(a.RoomContractSets, s
                    => Assert.All(s.Rooms, r
                        => Assert.Equal(contractKindCommissionOptions.Value.CreditCardPaymentsCommission, r.Rate.Commission))));
            Assert.All(result, a
                => Assert.All(a.RoomContractSets, s
                    => Assert.All(s.Rooms, r
                        => Assert.True(CommissionApplied(r.Rate.NetPrice.Amount, r.Rate.FinalPrice.Amount, r.Rate.Commission)))));
        }


        [Fact]
        public async Task Wide_availability_step_align_prices_should_apply_zero_commission_and_return_success()
        {
            var availabilityDetails = new List<AccommodationAvailabilityResult>()
            {
                new AccommodationAvailabilityResult(Guid.NewGuid(),
                    "jumeirah", DateTimeOffset.Now, "id", roomContractSets, 0m,
                    Decimal.MaxValue, DateTimeOffset.Now, DateTimeOffset.Now, "htId", "supplierAccommodationCode",
                    "countryHtId", "localityHtId", 2, "KZ")
            };
            var agencyId = 2;
            var agentContext = new AgentContext(1, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, agencyId,
                string.Empty, true, InAgencyPermissions.All, string.Empty, string.Empty, string.Empty, 2, new List<int>());

            var result = await _wideAvailabilityPriceProcessor.AlignPrices(availabilityDetails, agentContext);

            Assert.All(result, a
                => Assert.All(a.RoomContractSets, r
                    => Assert.Equal(ZeroCommission, r.Rate.Commission)));
            Assert.All(result, a
                => Assert.All(a.RoomContractSets, r
                    => Assert.True(CommissionApplied(r.Rate.NetPrice.Amount, r.Rate.FinalPrice.Amount, r.Rate.Commission))));
            Assert.All(result, a
                => Assert.All(a.RoomContractSets, s
                    => Assert.All(s.Rooms, r
                        => Assert.Equal(ZeroCommission, r.Rate.Commission))));
            Assert.All(result, a
                => Assert.All(a.RoomContractSets, s
                    => Assert.All(s.Rooms, r
                        => Assert.True(CommissionApplied(r.Rate.NetPrice.Amount, r.Rate.FinalPrice.Amount, r.Rate.Commission)))));
        }


        [Fact]
        public async Task Room_selection_step_align_prices_should_apply_zero_commission_and_return_success()
        {
            var availabilityDetails = new SingleAccommodationAvailability("id", DateTimeOffset.Now, roomContractSets, "htId",
                "countryHtId", "localityHtId", 2, "KZ", "jumeirah");
            var agencyId = 2;
            var agentContext = new AgentContext(1, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, agencyId,
                string.Empty, true, InAgencyPermissions.All, string.Empty, string.Empty, string.Empty, 2, new List<int>());

            var result = await _roomSelectionPriceProcessor.AlignPrices(availabilityDetails, agentContext);

            Assert.All(result.RoomContractSets, r
                => Assert.Equal(ZeroCommission, r.Rate.Commission));
            Assert.All(result.RoomContractSets, r
                => Assert.True(CommissionApplied(r.Rate.NetPrice.Amount, r.Rate.FinalPrice.Amount, r.Rate.Commission)));
            Assert.All(result.RoomContractSets, s
                => Assert.All(s.Rooms, r
                    => Assert.Equal(ZeroCommission, r.Rate.Commission)));
            Assert.All(result.RoomContractSets, s
                => Assert.All(s.Rooms, r
                    => Assert.True(CommissionApplied(r.Rate.NetPrice.Amount, r.Rate.FinalPrice.Amount, r.Rate.Commission))));
        }


        [Fact]
        public async Task Booking_step_align_prices_should_apply_zero_commission_and_return_success()
        {
            var availabilityDetails = new RoomContractSetAvailability("id", DateTimeOffset.Now, DateTimeOffset.Now, 7,
                new SlimAccommodation(), roomContractSets[0], new List<PaymentTypes>(), "countryHtId", "localityHtId",
                "evaluationToken", 2, "KZ", "jumeirah");
            var agencyId = 2;
            var agentContext = new AgentContext(1, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, agencyId,
                string.Empty, true, InAgencyPermissions.All, string.Empty, string.Empty, string.Empty, 2, new List<int>());

            var result = await _bookingEvaluationPriceProcessor.AlignPrices(availabilityDetails, agentContext);

            Assert.Equal(ZeroCommission, result.RoomContractSet.Rate.Commission);
            Assert.True(CommissionApplied(result.RoomContractSet.Rate.NetPrice.Amount,
                result.RoomContractSet.Rate.FinalPrice.Amount, result.RoomContractSet.Rate.Commission));
            Assert.All(result.RoomContractSet.Rooms, r
                    => Assert.Equal(ZeroCommission, r.Rate.Commission));
            Assert.All(result.RoomContractSet.Rooms, r
                    => Assert.True(CommissionApplied(r.Rate.NetPrice.Amount, r.Rate.FinalPrice.Amount, r.Rate.Commission)));
        }


        private bool CommissionApplied(decimal netPrice, decimal finalPrice, decimal commission)
            => finalPrice == netPrice * (100 + commission) / 100;


        private void SetupInitialData()
        {
            _edoContextMock
                .Setup(c => c.Agents)
                .Returns(DbSetMockProvider.GetDbSetMock(agents));

            _edoContextMock
                .Setup(c => c.AgentAgencyRelations)
                .Returns(DbSetMockProvider.GetDbSetMock(relations));

            _edoContextMock
                .Setup(c => c.Countries)
                .Returns(DbSetMockProvider.GetDbSetMock(countries));

            _edoContextMock
                .Setup(c => c.Agencies)
                .Returns(DbSetMockProvider.GetDbSetMock(agencies));
        }


        private readonly List<RoomContractSet> roomContractSets = new List<RoomContractSet>()
        {
            new RoomContractSet(Guid.NewGuid(), new Rate(new MoneyAmount(120m, Currencies.USD), new MoneyAmount(100m, Currencies.USD)),
                new Deadline(null, new List<CancellationPolicy>(), new List<string>(), true),
                new List<RoomContract>() {
                    new RoomContract(BoardBasisTypes.AllInclusive, "full", 1, true, true, "test", new List<KeyValuePair<string, string>>(),
                        new List<DailyRate>(), new Rate(new MoneyAmount(120m, Currencies.USD), new MoneyAmount(100m, Currencies.USD)),
                        2, new List<int>(), RoomTypes.Double, true,
                        new Deadline(null, new List<CancellationPolicy>(), new List<string>(), true), true)
                }, true, null, "jumeirah", new List<string>(), true, true)
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

        private readonly List<Country> countries = new()
        {
            new Country
            {
                Code = "RU"
            },
            new Country
            {
                Code = "KZ"
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

        private const decimal ZeroCommission = 0m;

        private readonly Mock<EdoContext> _edoContextMock;
        private readonly Mock<ITokenInfoAccessor> _tokenInfoAccessorMock;
        private readonly IAgentContextService _agentContextService;
        private readonly IRoomSelectionPriceProcessor _roomSelectionPriceProcessor;
        private readonly IBookingEvaluationPriceProcessor _bookingEvaluationPriceProcessor;
        private readonly IWideAvailabilityPriceProcessor _wideAvailabilityPriceProcessor;
        private readonly IOptions<ContractKindCommissionOptions> contractKindCommissionOptions;
    }
}