using System.Collections.Generic;
using System.Linq;
using HappyTravel.Edo.Api.Models.Accommodations;

namespace HappyTravel.Edo.Api.Extensions
{
    public static class AvailabilitySearchFilterExtensions
    {
        public static IEnumerable<WideAvailabilityResult> ApplyTo(this AvailabilitySearchFilter options, IQueryable<WideAvailabilityResult> queryable)
        {
            if (options.MinPrice.HasValue)
                queryable = queryable.Where(a => a.MinPrice >= options.MinPrice);

            if (options.MaxPrice.HasValue)
                queryable = queryable.Where(a => a.MaxPrice <= options.MaxPrice);

            if (options.BoardBasisTypes is not null && options.BoardBasisTypes.Any())
                queryable = queryable.Where(a => a.RoomContractSets.Any(rcs => rcs.Rooms.Any(r => options.BoardBasisTypes.Contains(r.BoardBasis))));

            if (options.Ratings is not null && options.Ratings.Any())
                queryable = queryable.Where(a => options.Ratings.Contains(a.Accommodation.Rating));
            
            queryable = queryable.Where(a => a.RoomContractSets.Any());

            if (options.Order == "price")
            {
                queryable = options.Direction switch
                {
                    "asc" => queryable.OrderBy(x => x.MinPrice),
                    "desc" => queryable.OrderByDescending(x => x.MinPrice),
                    _ => queryable
                };
            }

            return queryable
                .Skip(options.Skip)
                .Take(options.Top);
        }
    }
}