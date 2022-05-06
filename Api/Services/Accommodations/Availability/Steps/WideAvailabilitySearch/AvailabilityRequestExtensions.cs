using System;
using System.Text.Json;
using HappyTravel.Edo.Api.Models.Availabilities;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch;

public static class AvailabilityRequestExtensions
{
    public static string ComputeHash(this AvailabilityRequest request)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(request);
        return Convert.ToBase64String(bytes);
    }
}