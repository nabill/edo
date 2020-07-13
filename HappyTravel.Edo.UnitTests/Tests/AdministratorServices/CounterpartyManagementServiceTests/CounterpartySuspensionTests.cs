using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.UnitTests.Utility;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.AdministratorServices.CounterpartyManagementServiceTests
{
    public class CounterpartySuspensionTests
    {
        public CounterpartySuspensionTests()
        {
            _mockCreationHelper = new AdministratorServicesMockCreationHelper();
        }


        [Fact]
        public async Task Suspension_of_not_existing_counterparty_should_fail()
        {
            var context = _mockCreationHelper.GetContextMock();
            var counterpartyManagementService = _mockCreationHelper.GetCounterpartyManagementService(context.Object);

            var (_, isFailure, error) = await counterpartyManagementService.SuspendCounterparty(8);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Suspension_of_not_active_counterparty_should_fail()
        {
            var context = _mockCreationHelper.GetContextMock().Object;
            var counterpartyManagementService = _mockCreationHelper.GetCounterpartyManagementService(context);
            
            var (_, isFailure, error) = await counterpartyManagementService.SuspendCounterparty(2);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task Suspension_of_counterparty_should_suspend_all_dependants()
        {
            var context = _mockCreationHelper.GetContextMock().Object;
            var counterpartyManagementService = _mockCreationHelper.GetCounterpartyManagementService(context);
            
            var (_, isFailure, error) = await counterpartyManagementService.SuspendCounterparty(1);
        
            var counterparty = context.Counterparties.First(c => c.Id == 1);
            Assert.False(isFailure);
            Assert.True(!counterparty.IsActive);
        }


        private readonly AdministratorServicesMockCreationHelper _mockCreationHelper;
    }
}