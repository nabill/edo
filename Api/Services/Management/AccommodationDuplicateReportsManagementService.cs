using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Services.Connectors;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.AccommodationMappings;
using HappyTravel.EdoContracts.Accommodations;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Management
{
    public class AccommodationDuplicateReportsManagementService : IAccommodationDuplicateReportsManagementService
    {
        public AccommodationDuplicateReportsManagementService(EdoContext context, IDataProviderFactory dataProviderFactory)
        {
            _context = context;
            _dataProviderFactory = dataProviderFactory;
        }


        public Task<List<SlimAccommodationDuplicateReportInfo>> Get()
        {
            return (from report in _context.AccommodationDuplicateReports
                join agent in _context.Agents on report.ReporterAgentId equals agent.Id
                orderby report.Created descending
                select new SlimAccommodationDuplicateReportInfo(report.Id,
                    report.Created,
                    report.ApprovalState,
                    agent.FirstName,
                    report.Accommodations))
                .ToListAsync();
        }


        public async Task<Result<AccommodationDuplicateReportInfo>> Get(int reportId, string languageCode)
        {
            var report = await _context.AccommodationDuplicateReports.SingleOrDefaultAsync(r => r.Id == reportId);
            if (report == default)
                return Result.Failure<AccommodationDuplicateReportInfo>("Could not find a report");

            var accommodations = new List<ProviderData<AccommodationDetails>>(report.Accommodations.Count);
            foreach (var providerAccommodationId in report.Accommodations)
            {
                var (_, isFailure, accommodationDetails, result) = await _dataProviderFactory
                    .Get(providerAccommodationId.DataProvider)
                    .GetAccommodation(providerAccommodationId.Id, languageCode);
                
                if(isFailure)
                    return Result.Failure<AccommodationDuplicateReportInfo>($"Could not find accommodation: {result.Detail}");
                
                accommodations.Add(ProviderData.Create(providerAccommodationId.DataProvider, accommodationDetails));
            }
            
            return new AccommodationDuplicateReportInfo(report.Id, report.Created, report.ApprovalState, accommodations);
        }


        public async Task<Result> Approve(int reportId)
        {
            var report = await _context.AccommodationDuplicateReports.SingleOrDefaultAsync(r => r.Id == reportId);
            if (report == default)
                return Result.Failure("Could not find a report");
            
            var duplicates = await _context.AccommodationDuplicates.Where(d => d.ParentReportId == reportId).ToListAsync();

            report.ApprovalState = AccommodationDuplicateReportState.Approved;
            _context.Update(report);

            foreach (var duplicate in duplicates)
            {
                duplicate.IsApproved = true;
            }

            await _context.SaveChangesAsync();
            return Result.Success();
        }
        
        
        public async Task<Result> Disapprove(int reportId)
        {
            var report = await _context.AccommodationDuplicateReports.SingleOrDefaultAsync(r => r.Id == reportId);
            if (report == default)
                return Result.Failure("Could not find a report");
            
            var duplicates = await _context.AccommodationDuplicates.Where(d => d.ParentReportId == reportId).ToListAsync();

            report.ApprovalState = AccommodationDuplicateReportState.Disapproved;
            _context.Update(report);

            foreach (var duplicate in duplicates)
            {
                duplicate.IsApproved = false;
            }

            await _context.SaveChangesAsync();
            return Result.Success();
        }
        
        private readonly EdoContext _context;
        private readonly IDataProviderFactory _dataProviderFactory;
    }
}