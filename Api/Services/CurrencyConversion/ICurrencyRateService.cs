using System.Threading.Tasks;
using HappyTravel.EdoContracts.General.Enums;

namespace HappyTravel.Edo.Api.Services.CurrencyConversion
{
    public interface ICurrencyRateService
    {
        ValueTask<decimal> Get(Currencies source, Currencies target);
    }
}