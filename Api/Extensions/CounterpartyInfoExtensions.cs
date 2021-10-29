using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Data.Agents;

namespace HappyTravel.Edo.Api.Extensions
{
    public static class CounterpartyInfoExtensions
    {
        public static CounterpartyInfo ToCounterpartyInfo(this Counterparty counterparty, string markupFormula = null)
            => new (id: counterparty.Id,
                name: counterparty.Name,
                isContractUploaded: counterparty.IsContractUploaded,
                markupFormula: markupFormula);
    }
}
