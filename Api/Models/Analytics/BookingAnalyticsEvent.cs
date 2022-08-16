using System;
using HappyTravel.MapperContracts.Public.Accommodations.Internals;

namespace HappyTravel.Edo.Api.Models.Analytics;

public record BookingAnalyticsEvent
{
    /// <summary>
    /// Event datetime
    /// </summary>
    public DateTimeOffset DateTime { get; set; }
    
    /// <summary>
    /// Event id
    /// </summary>
    public int EventId { get; set; }
    
    /// <summary>
    /// Agency id
    /// </summary>
    public int AgencyId { get; set; }
    
    /// <summary>
    ///  Agent id
    /// </summary>
    public int AgentId { get; set; }
    
    /// <summary>
    /// Country name
    /// </summary>
    public string? Country { get; set; }
    
    /// <summary>
    /// Locality name
    /// </summary>
    public string? Locality { get; set; }
    
    /// <summary>
    /// Accommodation name
    /// </summary>
    public string? Accommodation { get; set; }
    
    /// <summary>
    /// Supplier code
    /// </summary>
    public string? SupplierCode { get; set; }
    
    /// <summary>
    /// Total price
    /// </summary>
    public decimal? TotalPrice { get; set; }
    
    /// <summary>
    ///  Geo point
    /// </summary>
    public GeoPoint? GeoPoint { get; set; }

    /// <summary>
    /// Agency name
    /// </summary>
    public string AgencyName { get; set; } = string.Empty;
    
    /// <summary>
    /// Agent name
    /// </summary>
    public string AgentName { get; set; } = string.Empty;
}