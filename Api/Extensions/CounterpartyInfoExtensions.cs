using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Data.Agents;

namespace HappyTravel.Edo.Api.Extensions
{
    public static class CounterpartyInfoExtensions
    {
        public static CounterpartyInfo ToCounterpartyInfo(this Counterparty counterparty, string countryNames, string languageCode, string markupFormula = null)
            => new (id: counterparty.Id,
                name: counterparty.Name,
                address: counterparty.Address,
                billingEmail: counterparty.BillingEmail,
                city: counterparty.City,
                countryCode: counterparty.CountryCode,
                countryName: LocalizationHelper.GetValueFromSerializedString(countryNames, languageCode),
                fax: counterparty.Fax,
                phone: counterparty.Phone,
                postalCode: counterparty.PostalCode,
                website: counterparty.Website,
                vatNumber: counterparty.VatNumber,
                isContractUploaded: counterparty.IsContractUploaded,
                markupFormula: markupFormula);
    }
}
