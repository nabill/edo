using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.AdministratorServices.Mapper.AccommodationManagementServices;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Mapping;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.MapperContracts.Public.Accommodations;
using HappyTravel.MapperContracts.Public.Management.Accommodations.DetailedAccommodations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public class FixHtIdService : IFixHtIdService
    {
        public FixHtIdService(EdoContext context,
            IAccommodationMapperClient client,
            ILogger<FixHtIdService> logger)
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

            var accommodationsCache = new Dictionary<(string, string), Result<Accommodation, ProblemDetails>>();

            foreach (var booking in bookingsWithEmptyHtId)
            {
                if (accommodationsCache.TryGetValue((booking.SupplierCode, booking.AccommodationId), out var result))
                {
                    UpdateHtId(booking, result);
                    continue;
                }

                var accommodationResult = await _client.GetAccommodation(booking.SupplierCode, booking.AccommodationId, booking.LanguageCode);
                accommodationsCache.Add((booking.SupplierCode, booking.AccommodationId), accommodationResult);
                UpdateHtId(booking, accommodationResult);
            }

            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();

            var errors = accommodationsCache.Where(c => c.Value.IsFailure)
                .ToDictionary(e => $"{e.Key.Item1}:{e.Key.Item2}", e => e.Value.Error.Detail);

            if (errors.Any())
                _logger.LogError("Updating errors {Errors}", errors);

            _logger.LogInformation("Updating {Count} completed", bookingsWithEmptyHtId.Count);


            void UpdateHtId(Booking booking, Result<Accommodation, ProblemDetails> accommodationResult)
            {
                if (accommodationResult.IsFailure)
                    return;

                booking.HtId = accommodationResult.Value.HtId;
                _context.Bookings.Attach(booking).Property(b => b.HtId).IsModified = true;
            }
        }


        private readonly EdoContext _context;
        private readonly IAccommodationMapperClient _client;
        private readonly ILogger<FixHtIdService> _logger;
    }
}