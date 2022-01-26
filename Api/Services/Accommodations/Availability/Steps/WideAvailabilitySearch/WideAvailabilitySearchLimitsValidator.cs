using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Availabilities;
using HappyTravel.MapperContracts.Internal.Mappings.Internals;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch;

public class WideAvailabilitySearchLimitsValidator : AbstractValidator<AvailabilityRequest>
{
    public WideAvailabilitySearchLimitsValidator(SearchLimits limits, List<Location> locations)
    {
        RuleFor(r => r.RoomDetails.Count).LessThanOrEqualTo(limits.MaxRoomsCount);
        
        RuleFor(r => r.HtIds)
            .Must(_ => IsLimitNotExceeded(() => locations.Select(l => l.CountryHtId).Distinct().Count(), limits.MaxCountriesCount))
            .WithMessage("Countries count limit exceeded");

        RuleFor(r => r.HtIds)
            .Must(_ => IsLimitNotExceeded(() => locations.Select(l => l.LocalityHtId).Distinct().Count(), limits.MaxLocalitiesCount))
            .WithMessage("Localities limit exceeded");

        RuleFor(r => r.HtIds)
            .Must(_ => IsLimitNotExceeded(() => locations.Count, limits.MaxLocationsCount))
            .WithMessage("Locations limit exceeded");
        
        RuleForEach(r => r.RoomDetails)
            .Must(r => IsLimitNotExceeded(() => r.AdultsNumber + r.ChildrenAges?.Count ?? 0, limits.MaxGuestsCount))
            .WithMessage("Guests limit exceeded");
    }


    private static bool IsLimitNotExceeded(Func<int> func, int limit) 
        => func() <= limit;
}