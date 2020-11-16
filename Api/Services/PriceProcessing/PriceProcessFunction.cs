using System.Threading.Tasks;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.Api.Services.PriceProcessing
{
    public delegate ValueTask<MoneyAmount> PriceProcessFunction(MoneyAmount price);
}