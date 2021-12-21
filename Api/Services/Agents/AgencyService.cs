using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentValidation;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Locations;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Money.Enums;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public class AgencyService : IAgencyService
    {
        public AgencyService(IDateTimeProvider dateTimeProvider,
            ILocalityInfoService localityInfoService,
            EdoContext context)
        {
            _dateTimeProvider = dateTimeProvider;
            _context = context;
            _localityInfoService = localityInfoService;
        }


        public async Task<Result<AgencyInfo>> Create(RegistrationAgencyInfo agencyInfo, int? parentAgencyId)
            => await Create(agencyInfo.Name, agencyInfo.Address, agencyInfo.BillingEmail, agencyInfo.Fax,
                agencyInfo.Phone, agencyInfo.PostalCode, agencyInfo.Website, agencyInfo.VatNumber,
                parentAgencyId, agencyInfo.LegalAddress, agencyInfo.PreferredPaymentMethod, agencyInfo.LocalityHtId);


        private async Task<Result<AgencyInfo>> Create(string name, string address, string billingEmail, string fax, string phone,
            string postalCode, string website, string vatNumber, int? parentAgencyId, string legalAddress,
            PaymentTypes preferredPaymentMethod, string localityHtId)
        {
            var ancestors = new List<int>();
            var (_, isFailure, localityInfo, error) = await _localityInfoService.GetLocalityInfo(localityHtId);
            if (isFailure)
                return Result.Failure<AgencyInfo>(error);

            if (parentAgencyId is not null)
            {
                var parentAncestors = await _context.Agencies
                    .Where(a => a.Id == parentAgencyId.Value)
                    .Select(a => a.Ancestors ?? new List<int>(0))
                    .SingleAsync();

                ancestors.AddRange(parentAncestors);
                ancestors.Add(parentAgencyId.Value);
            }

            var now = _dateTimeProvider.UtcNow();
            var agency = new Agency
            {
                Name = name,
                Created = now,
                Modified = now,
                ParentId = parentAgencyId,
                Address = address,
                BillingEmail = billingEmail,
                Fax = fax,
                Phone = phone,
                PostalCode = postalCode,
                Website = website,
                VatNumber = vatNumber,
                // Hardcode because we only support USD
                PreferredCurrency = Currencies.USD,
                Ancestors = ancestors,
                LegalAddress = legalAddress,
                PreferredPaymentMethod = preferredPaymentMethod,
                LocalityHtId = localityHtId,
                City = localityInfo.LocalityName,
                CountryCode = localityInfo.CountryIsoCode,
                CountryHtId = localityInfo.CountryHtId
            };
            _context.Agencies.Add(agency);

            await _context.SaveChangesAsync();
            return (await GetAgencyInfo(agency.Id)).Value;
        }


        public async Task<Result<SlimAgencyInfo>> Get(AgentContext agent, string languageCode = LocalizationHelper.DefaultLanguageCode)
        {
            return await GetAgencyInfo(agent.AgencyId, languageCode)
                .Map(agencyInfo => new SlimAgencyInfo(name: agencyInfo.Name,
                    address: agencyInfo.Address,
                    billingEmail: agencyInfo.BillingEmail,
                    city: agencyInfo.City,
                    countryCode: agencyInfo.CountryCode,
                    countryName: agencyInfo.CountryName,
                    fax: agencyInfo.Fax,
                    phone: agencyInfo.Phone,
                    postalCode: agencyInfo.PostalCode,
                    website: agencyInfo.Website,
                    vatNumber: agencyInfo.VatNumber,
                    defaultPaymentType: agencyInfo.DefaultPaymentType,
                    ancestors: agencyInfo.Ancestors,
                    countryHtId: agencyInfo.CountryHtId,
                    localityHtId: agencyInfo.LocalityHtId,
                    verificationState: agencyInfo.VerificationState,
                    verificationDate: agencyInfo.VerificationDate,
                    legalAddress: agencyInfo.LegalAddress,
                    preferredPaymentMethod: agencyInfo.PreferredPaymentMethod,
                    isContractUploaded: agencyInfo.IsContractUploaded));
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
                var agencyRecord = await _context.Agencies.SingleAsync(a => a.Id == agent.AgencyId);

                agencyRecord.Address = editAgencyRequest.Address;
                agencyRecord.Phone = editAgencyRequest.Phone;
                agencyRecord.Fax = editAgencyRequest.Fax;
                agencyRecord.PostalCode = editAgencyRequest.PostalCode;
                agencyRecord.Website = editAgencyRequest.Website;
                agencyRecord.BillingEmail = editAgencyRequest.BillingEmail;
                agencyRecord.VatNumber = editAgencyRequest.VatNumber;
                agencyRecord.PreferredPaymentMethod = editAgencyRequest.PreferredPaymentMethod;

                agencyRecord.Modified = _dateTimeProvider.UtcNow();

                _context.Update(agencyRecord);
                await _context.SaveChangesAsync();
            }


            Task<Result<SlimAgencyInfo>> GetUpdatedAgencyInfo()
                => Get(agent, languageCode);
        }


        private async Task<Result<AgencyInfo>> GetAgencyInfo(int agencyId, string languageCode = LocalizationHelper.DefaultLanguageCode)
        {
            var agencyInfo = await (
                    from a in _context.Agencies
                    join c in _context.Countries on a.CountryCode equals c.Code
                    join ra in _context.Agencies on a.Ancestors.Any() ? a.Ancestors[0] : a.Id equals ra.Id
                    from markupFormula in _context.DisplayMarkupFormulas.Where(f => f.AgencyId == a.Id && f.AgentId == null).DefaultIfEmpty() 
                    where a.Id == agencyId
                    select a.ToAgencyInfo(a.ContractKind, ra.VerificationState, ra.Verified, c.Names, languageCode, markupFormula == null ? string.Empty : markupFormula.DisplayFormula))
                .SingleOrDefaultAsync();

            return agencyInfo.Equals(default) 
                ? Result.Failure<AgencyInfo>("Could not find specified agency")
                : agencyInfo;
        }


        private readonly ILocalityInfoService _localityInfoService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly EdoContext _context;
    }
}