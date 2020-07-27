using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HappyTravel.Edo.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HappyTravel.Edo.Api.Services.Accommodations.Mappings
{
    public class AccommodationMappingServiceWarmUpHostedService : BackgroundService
    {
        public AccommodationMappingServiceWarmUpHostedService(AccommodationDuplicatesCacheService duplicatesCacheService,
            IServiceScopeFactory serviceScopeFactory)
        {
            _duplicatesCacheService = duplicatesCacheService;
            _serviceScopeFactory = serviceScopeFactory;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var serviceScope = _serviceScopeFactory.CreateScope();
            var context = serviceScope.ServiceProvider.GetRequiredService<EdoContext>();

            var duplicates = (await context.AccommodationDuplicateReports
                    .Select(dr => new
                    {
                        Accommodation = dr.Accommodation,
                        Duplicates = dr.Duplicates
                    })
                    .ToListAsync(cancellationToken: stoppingToken))
                .SelectMany(dr => dr.Duplicates.Union(new[] {dr.Accommodation}));
            
            foreach (var duplicate in duplicates)
            {
                _duplicatesCacheService.SetDuplicate(duplicate.DataProvider, duplicate.AccommodationId);
            }
        }
        
        private readonly AccommodationDuplicatesCacheService _duplicatesCacheService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
    }
}