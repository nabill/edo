using System;
using System.Text.Json;
using HappyTravel.Edo.Api.Models.Company;
using HappyTravel.Edo.Data;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Data.StaticData;
using Microsoft.EntityFrameworkCore;
using HappyTravel.Money.Enums;
using HappyTravel.Edo.Api.Infrastructure.ModelExtensions;
using HappyTravel.Edo.Api.Services.Company;
using System.Threading;
using HappyTravel.Edo.Api.Infrastructure;
using FluentValidation;

namespace Api.AdministratorServices
{
    public class CompanyInfoService : ICompanyInfoService
    {
        public CompanyInfoService(EdoContext context, IDoubleFlow flow)
        {
            _context = context;
            _flow = flow;
        }


        public async Task<Result> Update(CompanyInfo companyInfo, CancellationToken cancellationToken)
        {
            return await Validate(companyInfo)
                .Tap(UpdateInfo)
                .Tap(SaveCache);


            Result Validate(CompanyInfo companyInfo)
            {
                return GenericValidator<CompanyInfo>.Validate(v =>
                {
                    v.RuleFor(i => i.Address).NotEmpty();
                    v.RuleFor(i => i.Email).EmailAddress().When(i => !string.IsNullOrWhiteSpace(i.Email));
                    v.RuleFor(i => i.Phone).NotEmpty();
                    v.RuleFor(i => i.Name).NotEmpty();
                    v.RuleFor(i => i.Country).NotEmpty();
                    v.RuleFor(i => i.City).NotEmpty();
                    v.RuleFor(i => i.PostalCode).NotEmpty();
                    v.RuleFor(i => i.Trn).NotEmpty();
                    v.RuleFor(i => i.TradeLicense).NotEmpty();
                    v.RuleFor(i => i.AvailableCurrencies).NotEmpty();
                    v.RuleFor(i => i.DefaultCurrency).NotEmpty();
                }, companyInfo);
            }


            async Task UpdateInfo()
            {
                using var jsonDocInfo = JsonDocument.Parse(JsonSerializer.Serialize(companyInfo));

                var staticData = await _context.StaticData
                        .SingleOrDefaultAsync(sd => sd.Type == StaticDataTypes.CompanyInfo, cancellationToken);
                if (staticData == default)
                {
                    staticData = new StaticData
                    {
                        Type = StaticDataTypes.CompanyInfo,
                        Data = jsonDocInfo
                    };

                    _context.Add(staticData);
                }
                else
                {
                    staticData.Data = jsonDocInfo;
                    _context.Update(staticData);
                }

                await _context.SaveChangesAsync(cancellationToken);
            }


            async Task SaveCache()
                => await _flow.SetAsync(_flow.BuildKey(nameof(CompanyService), nameof(CompanyService.GetCompanyInfo)), companyInfo, CompanyInfoCacheLifeTime, cancellationToken);
        }


        public async Task<Result<CompanyInfo>> Get(CancellationToken cancellationToken)
        {
            var key = _flow.BuildKey(nameof(CompanyService), nameof(CompanyService.GetCompanyInfo));
            var info = await _flow.GetOrSetAsync(key, async () =>
            {
                var companyInfo = await _context.StaticData
                    .SingleOrDefaultAsync(sd => sd.Type == StaticDataTypes.CompanyInfo);

                if (companyInfo == default)
                    return default;

                return JsonSerializer.Deserialize<CompanyInfo>(companyInfo.Data.RootElement.ToString());
            }, CompanyInfoCacheLifeTime, cancellationToken);

            return info ?? Result.Failure<CompanyInfo>("Could not find company information");
        }


        public async Task<Result<CompanyAccountInfo>> GetDefaultBankAccount(Currencies currency, CancellationToken cancellationToken)
        {
            var key = _flow.BuildKey(nameof(CompanyService), nameof(GetDefaultBankAccount), currency.ToString());
            var account = await _flow.GetOrSetAsync(key, async () =>
            {
                var account = await _context.CompanyAccounts.Include(ca => ca.CompanyBank)
                    .SingleOrDefaultAsync(ca => ca.Currency == currency && ca.IsDefault);

                return account?.ToCompanyAccountInfo();
            }, CompanyInfoCacheLifeTime, cancellationToken);

            return account ?? Result.Failure<CompanyAccountInfo>($"Could not find a default bank account for {currency} currency");
        }


        private static readonly TimeSpan CompanyInfoCacheLifeTime = TimeSpan.FromHours(1);

        private readonly EdoContext _context;
        private readonly IDoubleFlow _flow;
    }
}