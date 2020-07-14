using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.AdministratorServices;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.UnitTests.Utility;
using HappyTravel.EdoContracts.General.Enums;
using HappyTravel.Money.Enums;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.AdministratorServices.CounterpartyManagementServiceTests
{
    public class CounterpartyManagementTests
    {
        public CounterpartyManagementTests()
        {
            var mockCreationHelper = new AdministratorServicesMockCreationHelper();
            _context = mockCreationHelper.GetContextMock().Object;
            _counterpartyManagementService = mockCreationHelper.GetCounterpartyManagementService(_context);
        }


        [Fact]
        public async Task Get_specified_counterparty_should_return_counterparty_infо()
        {
            var (_, isFailure, counterparty, error) = await _counterpartyManagementService.Get(1, "en");

            Assert.False(isFailure);
            Assert.True(counterparty.Name == "Test");
        }


        [Fact]
        public async Task Get_not_existed_counterparty_should_fail()
        {
            var (_, isFailure, counterparty, error) = await _counterpartyManagementService.Get(7, "en");
            
            Assert.True(isFailure);
        }


        [Fact]
        public async Task Get_counterparty_should_return_all_counterparties()
        {
            var counterpartyList = await _counterpartyManagementService.Get("en");

            Assert.True(counterpartyList.Count == 2);
        }


        [Fact]
        public async Task Get_counterparty_agencies_should_return_agencies()
        {
            var (_, isFailure, agencies, error) = await _counterpartyManagementService.GetAllCounterpartyAgencies(1);

            Assert.False(isFailure);
            Assert.True(agencies.Count == 3);
        }


        [Fact]
        public async Task Get_not_existed_counterparty_agencies_should_fail()
        {
            var (_, isFailure, _, _) = await _counterpartyManagementService.GetAllCounterpartyAgencies(7);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Counterparty_update_should_pass()
        {
            var counterpartyToUpdate = new CounterpartyEditRequest(
                name: "RenamedName",
                address: "changed address",
                countryCode: "AF",
                city: "changed city",
                phone: "79265748556",
                fax: "79265748336",
                postalCode: "changed code",
                preferredCurrency: Currencies.EUR,
                preferredPaymentMethod: PaymentMethods.Offline,
                website: "changed website",
                vatNumber: "changed vatNumber",
                billingEmail: "changed email"
            );

            var (_, isFailure, counterparty, error) = await _counterpartyManagementService.Update(counterpartyToUpdate, 1, "en");

            Assert.False(isFailure);
            Assert.True(counterparty.Name == "RenamedName");
            Assert.True(_context.Counterparties.Single(c => c.Id == 1).Name != "Test" );
        }


        [Fact]
        public async Task Update_not_existing_counterparty_should_fail()
        {
            var counterpartyToUpdate = new CounterpartyEditRequest();
            
            var (_, isFailure, _, _) = await _counterpartyManagementService.Update(counterpartyToUpdate, 1, "en");

            Assert.True(isFailure);
        }


        private readonly EdoContext _context;
        private readonly ICounterpartyManagementService _counterpartyManagementService;
    }
}