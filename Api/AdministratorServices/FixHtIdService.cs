using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Mapping;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.MapperContracts.Public.Accommodations;
using HappyTravel.SuppliersCatalog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public class FixHtIdService : IFixHtIdService
    {
        public FixHtIdService(EdoContext context, IAccommodationMapperClient client, ILogger<FixHtIdService> logger)
        {
            _context = context;
            _client = client;
            _logger = logger;
        }
        
        
        public async Task FillEmptyHtIds()
        {
            var bookingsWithEmptyHtId = await _context.Bookings
                .Where(b => string.IsNullOrEmpty(b.HtId))
                .ToListAsync();

            var accommodationsCache = new Dictionary<(int, string), Result<Accommodation, ProblemDetails>>();

            foreach (var booking in bookingsWithEmptyHtId)
            {
                if (accommodationsCache.TryGetValue(((int) booking.Supplier, booking.AccommodationId), out var result))
                {
                    UpdateHtId(booking, result);
                    continue;
                }
                
                var accommodationResult = await _client.GetAccommodation(booking.Supplier, booking.AccommodationId, booking.LanguageCode);
                accommodationsCache.Add(((int) booking.Supplier, booking.AccommodationId), accommodationResult);
                UpdateHtId(booking, accommodationResult);
            }
            
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();
            
            var errors = accommodationsCache.Where(c => c.Value.IsFailure)
                .ToDictionary(e => $"{e.Key.Item1}:{e.Key.Item2}", e => e.Value.Error.Detail);
            
            if (errors.Any())
                _logger.LogError("Updating errors {Errors}", errors);
            
            _logger.LogInformation("Updating {Count} completed", bookingsWithEmptyHtId.Count);
        }


        private void UpdateHtId(Booking booking, Result<Accommodation,ProblemDetails> accommodationResult)
        {
            if (accommodationResult.IsFailure)
                return;

            booking.HtId = accommodationResult.Value.HtId;
            _context.Bookings.Attach(booking).Property(b => b.HtId).IsModified = true;
        }


        private readonly EdoContext _context;
        private readonly IAccommodationMapperClient _client;
        private readonly ILogger<FixHtIdService> _logger;
    }
}