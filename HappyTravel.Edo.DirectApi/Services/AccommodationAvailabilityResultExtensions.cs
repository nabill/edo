﻿using System;
using System.Collections.Generic;
using System.Linq;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Services.Accommodations.Availability;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch;
using HappyTravel.Edo.Common.Enums.AgencySettings;

namespace HappyTravel.Edo.DirectApi.Services
{
    public static class AccommodationAvailabilityResultExtensions
    {
        public static List<WideAvailabilityResult> ToWideAvailabilityResults(this IEnumerable<AccommodationAvailabilityResult> list, AccommodationBookingSettings searchSettings) 
            => list.Select(x => x.ToWideAvailabilityResult(searchSettings)).ToList();


        private static WideAvailabilityResult ToWideAvailabilityResult(this AccommodationAvailabilityResult result, AccommodationBookingSettings searchSettings)
        {
            var roomContractSets = result.RoomContractSets
                .Select(r => r.ApplySearchSettings(searchSettings.IsSupplierVisible, searchSettings.IsDirectContractFlagVisible))
                .ToList();

            if (searchSettings.AprMode == AprMode.Hide)
                roomContractSets = roomContractSets.Where(r => !r.IsAdvancePurchaseRate).ToList();

            if (searchSettings.PassedDeadlineOffersMode == PassedDeadlineOffersMode.Hide)
                roomContractSets = roomContractSets.Where(r => r.Deadline.Date == null || r.Deadline.Date >= DateTime.UtcNow).ToList();

            return new WideAvailabilityResult(default,
                roomContractSets,
                result.MinPrice,
                result.MaxPrice,
                result.CheckInDate,
                result.CheckOutDate,
                searchSettings.IsSupplierVisible
                    ? result.Supplier
                    : null,
                result.HtId);
        }
    }
}