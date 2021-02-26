using System;
using System.Collections.Generic;
using System.Linq;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch
{
    public static class RoomContractSetListExtensions
    {
        public static List<RoomContractSet> ApplySearchFilters(this IEnumerable<RoomContractSet> roomContractSets,
            AccommodationBookingSettings searchSettings, IDateTimeProvider dateTimeProvider, DateTime checkInDate)
        {
            return roomContractSets.Where(roomSet => RoomContractSetSettingsChecker.IsAllowed(roomSet, checkInDate, searchSettings, dateTimeProvider))
                .ToList();
        }
        
        
        public static List<RoomContractSet> ToEdoRoomContractSets(this IEnumerable<EdoContracts.Accommodations.Internals.RoomContractSet> roomContractSets, Suppliers? supplier)
        {
            return roomContractSets
                .Select(rs => rs.ToRoomContractSet(supplier, rs.SystemTags))
                .ToList();
        }
    }
}