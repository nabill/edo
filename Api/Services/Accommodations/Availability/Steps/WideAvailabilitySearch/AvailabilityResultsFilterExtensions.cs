using System.Collections.Generic;
using System.Linq;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch
{
    public static class AvailabilityResultsFilterExtensions
    {
        public static IEnumerable<(Suppliers, AccommodationAvailabilityResult)> RemoveRepeatedAccommodations(this IEnumerable<(Suppliers, AccommodationAvailabilityResult)> results)
        {
            return results.Distinct(new AccommodationResultComparer());
        }
        
        private class AccommodationResultComparer : IEqualityComparer<(Suppliers, AccommodationAvailabilityResult Data)>
        {
            public bool Equals((Suppliers, AccommodationAvailabilityResult Data) result1, (Suppliers, AccommodationAvailabilityResult Data) result2)
            {
                if (string.IsNullOrWhiteSpace(result1.Data.DuplicateReportId) || string.IsNullOrWhiteSpace(result2.Data.DuplicateReportId))
                    return false;
                
                return result1.Data.DuplicateReportId == result2.Data.DuplicateReportId;
            }


            public int GetHashCode((Suppliers, AccommodationAvailabilityResult Data) result)
            {
                return result.Data.DuplicateReportId.GetHashCode();
            }
        }
    }
}