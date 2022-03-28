using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Models.Locations;
using HappyTravel.Edo.Data;
using Microsoft.EntityFrameworkCore;

namespace Api.AdministratorServices.Locations
{
    public class MarkupLocationService : IMarkupLocationService
    {
        public MarkupLocationService(EdoContext context, IDoubleFlow flow)
        {
            _context = context;
            _flow = flow;
        }


        public Task<List<Region>> GetRegions(string languageCode)
            => _flow.GetOrSetAsync(_flow.BuildKey(nameof(MarkupLocationService), RegionsKeyBase, languageCode), async ()
               => await _context.Regions
                    .Select(r => new Region(r.Id, r.Names.RootElement.GetProperty(languageCode).GetString()))
                    .ToListAsync(), DefaultLocationCachingTime);


        private static TimeSpan DefaultLocationCachingTime => TimeSpan.FromDays(1);

        private const string RegionsKeyBase = "Regions";

        private readonly EdoContext _context;
        private readonly IDoubleFlow _flow;
    }
}