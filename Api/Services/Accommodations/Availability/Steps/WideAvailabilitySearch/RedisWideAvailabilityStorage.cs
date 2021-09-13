using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Common.Enums.AgencySettings;
using HappyTravel.SuppliersCatalog;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch
{
    public class RedisWideAvailabilityStorage : IWideAvailabilityStorage
    {
        public RedisWideAvailabilityStorage(IMultiProviderAvailabilityStorage multiProviderAvailabilityStorage, IDateTimeProvider dateTimeProvider, IWideAvailabilityAccommodationsStorage accommodationsStorage)
        {
            _multiProviderAvailabilityStorage = multiProviderAvailabilityStorage;
            _dateTimeProvider = dateTimeProvider;
            _accommodationsStorage = accommodationsStorage;
        }


        public async Task<List<(Suppliers SupplierKey, List<AccommodationAvailabilityResult> AccommodationAvailabilities)>> GetResults(Guid searchId, List<Suppliers> suppliers)
        {
            return  (await _multiProviderAvailabilityStorage.Get<List<AccommodationAvailabilityResult>>(searchId.ToString(), suppliers, true))
                .Where(t => t.Result != default)
                .ToList();
        }


        public async Task<List<WideAvailabilityResult>> GetFilteredResults(Guid searchId, AvailabilitySearchFilter filters, AccommodationBookingSettings searchSettings, List<Suppliers> suppliers, string languageCode)
        {
            var results = await GetResults(searchId, suppliers);
            var availabilities = results.SelectMany(r => r.AccommodationAvailabilities)
                .GroupBy(a => a.HtId)
                .Select(g => g.OrderBy(a => a.Created).First())
                .AsQueryable();

            if (searchSettings.AprMode == AprMode.Hide)
                availabilities = availabilities.Where(a => a.RoomContractSets.All(rcs => !rcs.IsAdvancePurchaseRate));
            
            if (searchSettings.PassedDeadlineOffersMode == PassedDeadlineOffersMode.Hide)
                availabilities = availabilities.Where(a => a.RoomContractSets.All(rcs => rcs.Deadline.Date == null || rcs.Deadline.Date >= _dateTimeProvider.UtcNow()));

            var htIds = availabilities.Select(a => a.HtId).ToList();
            await _accommodationsStorage.EnsureAccommodationsCached(htIds, languageCode);
            
            var queriable = availabilities.ToList().Select(a =>
            {
                var accommodation = _accommodationsStorage.GetAccommodation(a.HtId, languageCode);

                return new WideAvailabilityResult(accommodation,
                    a.RoomContractSets,
                    a.MinPrice,
                    a.MaxPrice,
                    a.CheckInDate,
                    a.CheckOutDate,
                    searchSettings.IsSupplierVisible
                        ? a.Supplier
                        : (Suppliers?)null,
                    a.HtId);
            }).AsQueryable();
            
            return filters.ApplyTo(queriable).ToList();
        }


        public Task SaveResults(Guid searchId, Suppliers supplier, List<AccommodationAvailabilityResult> results)
        {
            return _multiProviderAvailabilityStorage.Save(searchId.ToString(), results, supplier);
        }
        
        
        private readonly IMultiProviderAvailabilityStorage _multiProviderAvailabilityStorage;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IWideAvailabilityAccommodationsStorage _accommodationsStorage;
    }
}