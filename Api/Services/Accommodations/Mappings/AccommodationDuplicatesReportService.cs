using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.AccommodationMappings;

namespace HappyTravel.Edo.Api.Services.Accommodations.Mappings
{
    public class AccommodationDuplicatesReportService
    {
        public AccommodationDuplicatesReportService(EdoContext context, IDateTimeProvider dateTimeProvider,
            AccommodationDuplicatesCacheService cacheService)
        {
            _context = context;
            _dateTimeProvider = dateTimeProvider;
            _cacheService = cacheService;
        }

        
        public async Task Add(ReportAccommodationDuplicateRequest duplicateRequest, AgentContext agent)
        {
            var now = _dateTimeProvider.UtcNow();
            _context.AccommodationDuplicateReports.Add(new AccommodationDuplicateReport
            {
                Accommodation = duplicateRequest.Accommodation,
                Duplicates = duplicateRequest.Duplicates,
                ReporterAgentId = agent.AgentId,
                ReporterAgencyId = agent.AgencyId,
                Created = now,
                Modified = now,
            });

            foreach (var providerAccommodationId in duplicateRequest.Duplicates.Union(new []{ duplicateRequest.Accommodation}))
            {
                _cacheService.SetDuplicate(providerAccommodationId.DataProvider, providerAccommodationId.AccommodationId);
            }

            await _context.SaveChangesAsync();
        }


        private readonly EdoContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly AccommodationDuplicatesCacheService _cacheService;
    }
}