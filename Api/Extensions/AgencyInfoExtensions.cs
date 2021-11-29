using System;
using System.Collections.Generic;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.BookingEvaluation;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Agents;

namespace HappyTravel.Edo.Api.Extensions
{
    public static class AgencyInfoExtensions
    {
        public static AgencyInfo ToAgencyInfo(this Agency agency, ContractKind? contractKind,
            AgencyVerificationStates verificationState, DateTime? verificationDate, string countryNames, string languageCode, string markupFormula)
            => new AgencyInfo(name: agency.Name,
                id: agency.Id,
                address: agency.Address,
                billingEmail: agency.BillingEmail,
                city: agency.City,
                countryCode: agency.CountryCode,
                countryName: LocalizationHelper.GetValueFromSerializedString(countryNames, languageCode),
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
                preferredCurrency: agency.PreferredCurrency);


        public static AdminViewAgencyInfo ToAdminViewAgencyInfo(this Agency agency, AgencyVerificationStates verificationState,
            string accountManagerName, string countryNames, string languageCode)
            => new AdminViewAgencyInfo()
            {
                Id = agency.Id,
                Name = agency.Name,
                City = agency.City,
                CountryName = LocalizationHelper.GetValueFromSerializedString(countryNames, languageCode),
                Created = agency.Created,
                VerificationState = verificationState,
                AccountManagerName = accountManagerName,
                IsActive = agency.IsActive
            };
    }
}
