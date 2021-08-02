using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Data.Agents;

namespace HappyTravel.Edo.Api.Extensions
{
    public static class CounterpartyInfoExtensions
    {
        public static CounterpartyInfo ToCounterpartyInfo(this Counterparty counterparty, string countryNames, string languageCode, string markupFormula = null)
            => new(counterparty.Id,
                counterparty.Name,
                counterparty.LegalAddress,
                counterparty.Address,
                counterparty.BillingEmail,
                counterparty.City,
                counterparty.CountryCode,
                LocalizationHelper.GetValueFromSerializedString(countryNames, languageCode),
                counterparty.Fax,
                counterparty.Phone,
                counterparty.PostalCode,
                counterparty.Website,
                counterparty.VatNumber,
                counterparty.PreferredPaymentMethod,
                counterparty.IsContractUploaded,
                counterparty.State,
                counterparty.Verified,
                counterparty.IsActive,
                markupFormula);
    }
}
