using System.Threading.Tasks;
using HappyTravel.EdoContracts.General.Enums;

namespace HappyTravel.Edo.Api.Services.PriceProcessing
{
    public delegate ValueTask<(decimal Amount, Currencies Currency)> PriceProcessFunction(decimal price, Currencies currency);
}