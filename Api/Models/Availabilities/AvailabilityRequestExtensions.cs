using System;
using System.Text.Json;

namespace HappyTravel.Edo.Api.Models.Availabilities;

public static class AvailabilityRequestExtensions
{
    public static string ComputeHash(this AvailabilityRequest request)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(request);
        return Convert.ToBase64String(bytes);
    }
}