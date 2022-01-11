using System.Linq;
using HappyTravel.Edo.DirectApi.Models.Search;

namespace HappyTravel.Edo.DirectApi.Services.AvailabilitySearch
{
    internal static class RateExtensions
    {
        public static Rate MapFromEdoModel(this Api.Models.Accommodations.Rate rate) 
            => new Rate(finalPrice: rate.FinalPrice,
                gross: rate.Gross,
                discounts: rate.Discounts
                    .Select(d => new Discount(d.Percent, d.Description))
                    .ToList(),
                type: rate.Type,
                description: rate.Description);
    }
}