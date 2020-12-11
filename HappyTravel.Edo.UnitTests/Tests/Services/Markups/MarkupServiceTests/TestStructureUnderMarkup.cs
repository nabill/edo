using System.Threading.Tasks;
using HappyTravel.Edo.Api.Services.PriceProcessing;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Markups.MarkupServiceTests
{
    public class TestStructureUnderMarkup
    {
        public MoneyAmount Price { get; set; }

        public static async ValueTask<TestStructureUnderMarkup> Apply(TestStructureUnderMarkup initial, PriceProcessFunction processFunction)
        {
            var resultPrice = await processFunction(initial.Price);
            return new TestStructureUnderMarkup
            {
                Price = resultPrice
            };
        }
    }
}