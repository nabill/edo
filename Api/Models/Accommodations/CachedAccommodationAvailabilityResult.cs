using System;
using System.Collections.Generic;
using MongoDB.Bson;

namespace HappyTravel.Edo.Api.Models.Accommodations;

public record CachedAccommodationAvailabilityResult
{
    public ObjectId Id { get; init; }
    public Guid SearchId { get; init; }
    public string SupplierCode { get; init; } = string.Empty;
    public DateTimeOffset Created { get; init; }
    public string AvailabilityId { get; init; } = string.Empty;
    public List<RoomContractSet> RoomContractSets { get; init; } = new();
    public decimal MinPrice { get; init; }
    public decimal MaxPrice { get; init; }
    public DateTimeOffset CheckInDate { get; init; }
    public DateTimeOffset CheckOutDate { get; init; }
    public string HtId { get; init; } = string.Empty;
    public string SupplierAccommodationCode { get; init; } = string.Empty;
    public string CountryHtId { get; init; } = string.Empty;
    public string LocalityHtId { get; init; } = string.Empty;
    public int MarketId { get; init; }
    public string CountryCode { get; init; } = string.Empty;
}