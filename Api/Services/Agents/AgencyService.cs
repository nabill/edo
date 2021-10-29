﻿using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentValidation;
using HappyTravel.Edo.Api.AdministratorServices;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Data;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public class AgencyService : IAgencyService
    {
        public AgencyService(IAdminAgencyManagementService adminAgencyManagementService,
            IDateTimeProvider dateTimeProvider,
            EdoContext edoContext)
        {
            _adminAgencyManagementService = adminAgencyManagementService;
            _dateTimeProvider = dateTimeProvider;
            _edoContext = edoContext;
        }


        public Task<Result<SlimAgencyInfo>> Get(AgentContext agent, string languageCode = LocalizationHelper.DefaultLanguageCode)
        {
            return _adminAgencyManagementService.Get(agent.AgencyId, languageCode)
                .Map(a => new SlimAgencyInfo(
                    name: a.Name,
                    address: a.Address,
                    billingEmail: a.BillingEmail,
                    city: a.City,
                    countryCode: a.CountryCode,
                    countryName: a.CountryName,
                    fax: a.Fax,
                    phone: a.Phone,
                    postalCode: a.PostalCode,
                    website: a.Website,
                    vatNumber: a.VatNumber,
                    defaultPaymentType: a.DefaultPaymentType,
                    ancestors: a.Ancestors,
                    countryHtId: a.CountryHtId,
                    localityHtId: a.LocalityHtId,
                    verificationState: a.VerificationState,
                    verificationDate: a.VerificationDate,
                    legalAddress: a.LegalAddress,
                    preferredPaymentMethod: a.PreferredPaymentMethod));
        }


        public Task<Result<SlimAgencyInfo>> Edit(AgentContext agent, EditAgencyRequest editAgencyRequest,
            string languageCode = LocalizationHelper.DefaultLanguageCode)
        {
            return Validate()
                .Tap(UpdateAgencyRecord)
                .Bind(GetUpdatedAgencyInfo);


            Result Validate()
            {
                return GenericValidator<EditAgencyRequest>.Validate(v =>
                {
                    v.RuleFor(c => c.Address).NotEmpty();
                    v.RuleFor(c => c.Phone).NotEmpty();
                    v.RuleFor(c => c.BillingEmail).EmailAddress().When(i => !string.IsNullOrWhiteSpace(i.BillingEmail));
                }, editAgencyRequest);
            }


            async Task UpdateAgencyRecord()
            {
                var agencyRecord = await _edoContext.Agencies.SingleAsync(a => a.Id == agent.AgencyId);

                agencyRecord.Address = editAgencyRequest.Address;
                agencyRecord.Phone = editAgencyRequest.Phone;
                agencyRecord.Fax = editAgencyRequest.Fax;
                agencyRecord.PostalCode = editAgencyRequest.PostalCode;
                agencyRecord.Website = editAgencyRequest.Website;
                agencyRecord.BillingEmail = editAgencyRequest.BillingEmail;
                agencyRecord.VatNumber = editAgencyRequest.VatNumber;
                agencyRecord.PreferredPaymentMethod = editAgencyRequest.PreferredPaymentMethod;

                agencyRecord.Modified = _dateTimeProvider.UtcNow();

                _edoContext.Update(agencyRecord);
                await _edoContext.SaveChangesAsync();
            }


            Task<Result<SlimAgencyInfo>> GetUpdatedAgencyInfo()
                => Get(agent, languageCode);
        }


        private readonly IAdminAgencyManagementService _adminAgencyManagementService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly EdoContext _edoContext;
    }
}
