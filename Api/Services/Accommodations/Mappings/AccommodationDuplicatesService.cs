using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using FluentValidation;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.AccommodationMappings;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Accommodations.Mappings
{
    public class AccommodationDuplicatesService : IAccommodationDuplicatesService
    {
        public AccommodationDuplicatesService(EdoContext context, IDateTimeProvider dateTimeProvider, IDoubleFlow flow)
        {
            _context = context;
            _dateTimeProvider = dateTimeProvider;
            _flow = flow;
        }


        public async Task<Result> Report(ReportAccommodationDuplicateRequest duplicateRequest, AgentContext agent)
        {
            var now = _dateTimeProvider.UtcNow();
            
            return await Validate(duplicateRequest)
                .Map(SaveReport)
                .Tap(AddDuplicateRecords)
                .Tap(ResetCacheForAgent);


            static Result Validate(ReportAccommodationDuplicateRequest duplicateRequest)
            {
                return GenericValidator<ReportAccommodationDuplicateRequest>.Validate(v =>
                {
                    v.RuleFor(r => r.Duplicates).NotEmpty();
                    v.RuleForEach(r => r.Duplicates.Union(new[] {r.Accommodation}))
                        .ChildRules(d =>
                        {
                            d.RuleFor(d => d.Id).NotEmpty();
                            d.RuleFor(d => d.Supplier).IsInEnum();
                            d.RuleFor(d => d.Supplier)
                                .Must(p => p != Suppliers.Unknown)
                                .WithMessage("Provider code is required");
                        })
                        .OverridePropertyName("Accommodations");
                }, duplicateRequest);
            }


            async Task<AccommodationDuplicateReport> SaveReport()
            {
                var report = new AccommodationDuplicateReport
                {
                    Created = now,
                    Modified = now,
                    ReporterAgencyId = agent.AgencyId,
                    ReporterAgentId = agent.AgentId,
                    ApprovalState = AccommodationDuplicateReportState.PendingApproval,
                    Accommodations = duplicateRequest.Duplicates
                        .Union(new[] {duplicateRequest.Accommodation})
                        .ToList()
                };
                _context.Add(report);
                await _context.SaveChangesAsync();
                return report;
            }


            async Task AddDuplicateRecords(AccommodationDuplicateReport parentReport)
            {
                var allDuplicateIds = duplicateRequest.Duplicates
                    .Union(new[] {duplicateRequest.Accommodation})
                    .Select(a => a.ToString())
                    .ToList();

                var duplicateRecords = allDuplicateIds
                    .SelectMany(accommodationId1 => allDuplicateIds, (accommodationId1, accommodationId2) => new {accommodationId1, accommodationId2})
                    .Where(d => d.accommodationId1 != d.accommodationId2)
                    .Distinct();

                foreach (var duplicate in duplicateRecords)
                {
                    _context.AccommodationDuplicates.Add(new AccommodationDuplicate
                    {
                        AccommodationId1 = duplicate.accommodationId1,
                        AccommodationId2 = duplicate.accommodationId2,
                        ReporterAgentId = agent.AgentId,
                        ReporterAgencyId = agent.AgencyId,
                        ParentReportId = parentReport.Id,
                        IsApproved = false,
                        Created = now
                    });
                }

                await _context.SaveChangesAsync();
            }


            Task ResetCacheForAgent() => ResetCache(agent);
        }


        public Task<HashSet<SupplierAccommodationId>> Get(AgentContext agent)
        {
            return _flow.GetOrSetAsync(
                key: BuildKey(agent),
                getValueFunction: () => GetDuplicatesFromDb(agent),
                DuplicatesCacheLifeTime);
        }


        public async Task<Dictionary<SupplierAccommodationId, string>> GetDuplicateReports(List<SupplierAccommodationId> supplierAccommodationIds)
        {
            var accommodationIds = supplierAccommodationIds.Select(p => p.ToString()).ToList();
            var duplicates = await _context.AccommodationDuplicates
                .Where(d => accommodationIds.Contains(d.AccommodationId1) && d.IsApproved)
                .ToListAsync();
            
            var duplicateGroups = duplicates
                .GroupBy(d => d.AccommodationId1)
                .ToList();

            return duplicateGroups
                .Select(g => new
                {
                    Accommodation1 = g.Key,
                    ReportKey = string.Join("-", g.Select(a => a.ParentReportId).OrderBy(r => r))
                })
                .ToDictionary(
                    keySelector: duplicate => SupplierAccommodationId.FromString(duplicate.Accommodation1),
                    elementSelector: duplicate => duplicate.ReportKey
                );
        }


        private async Task<HashSet<SupplierAccommodationId>> GetDuplicatesFromDb(AgentContext agent)
        {
            return (await _context.AccommodationDuplicates
                    .Where(d => d.ReporterAgentId == agent.AgentId)
                    .Select(d => d.AccommodationId1)
                    .Distinct()
                    .Select(id => SupplierAccommodationId.FromString(id))
                    .ToListAsync())
                .ToHashSet();
        }


        private Task ResetCache(AgentContext agent)
            => _flow.RemoveAsync(BuildKey(agent));


        private string BuildKey(AgentContext agent)
            => _flow
                .BuildKey(nameof(AccommodationDuplicatesService), "Duplicates", agent.AgentId.ToString());


        private readonly EdoContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IDoubleFlow _flow;
        private static readonly TimeSpan DuplicatesCacheLifeTime = TimeSpan.FromMinutes(2);
    }
}