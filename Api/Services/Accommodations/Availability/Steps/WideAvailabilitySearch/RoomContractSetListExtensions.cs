using System.Collections.Generic;
using System.Linq;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch
{
    public static class RoomContractSetListExtensions
    {
        public static List<RoomContractSet> ToEdoRoomContractSets(this IEnumerable<EdoContracts.Accommodations.Internals.RoomContractSet> roomContractSets, Suppliers? supplier)
        {
            return roomContractSets
                .Select(rs => rs.ToRoomContractSet(supplier, rs.Tags))
                .ToList();
        }
    }
}