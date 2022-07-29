using System;
using System.Collections.Generic;
using System.Text.Json;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.BookingEvaluation;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.MultiLanguage;

namespace HappyTravel.Edo.Api.Extensions
{
    public static class AgencyInfoExtensions
    {
        public static AgencyInfo ToAgencyInfo(this Agency agency, ContractKind? contractKind,
            AgencyVerificationStates verificationState, DateTime? verificationDate, MultiLanguage<string> countryNames,
            string languageCode, string markupFormula, string? accountManagerName, int? accountManagerId)
            => new AgencyInfo(name: agency.Name,
                id: agency.Id,
                address: agency.Address,
                billingEmail: agency.BillingEmail,
                city: agency.City,
                countryCode: agency.CountryCode,
                countryName: countryNames.GetValueOrDefault(languageCode),
                fax: agency.Fax,
                phone: agency.Phone,
                postalCode: agency.PostalCode,
                website: agency.Website,
                vatNumber: agency.VatNumber,
                defaultPaymentType: BookingPaymentTypesHelper.GetDefaultPaymentType(contractKind),
                countryHtId: agency.CountryHtId,
                localityHtId: agency.LocalityHtId,
                ancestors: agency.Ancestors ?? new List<int>(),
                verificationState: verificationState,
                verificationDate: verificationDate,
                isActive: agency.IsActive,
                legalAddress: agency.LegalAddress,
                preferredPaymentMethod: agency.PreferredPaymentMethod,
                isContractUploaded: agency.IsContractUploaded,
                markupDisplayFormula: markupFormula,
                preferredCurrency: agency.PreferredCurrency,
                accountManagerName: accountManagerName,
                accountManagerId: accountManagerId,
                contractKind: agency.ContractKind,
                creditLimit: agency.CreditLimit);
    }
}
