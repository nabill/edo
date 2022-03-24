using System;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Models.Bookings;

public readonly struct AdministratorBookingStatusHistoryEntry
{
    public AdministratorBookingStatusHistoryEntry(int id, int bookingId, string userId, ApiCallerTypes apiCallerType, 
        int? agencyId, DateTimeOffset createdAt, BookingStatuses status, BookingChangeInitiators initiator, 
        BookingChangeSources source, BookingChangeEvents @event, string reason, string? agentName, string? adminName)
    {
        Id = id;
        BookingId = bookingId;
        UserId = userId;
        ApiCallerType = apiCallerType;
        AgencyId = agencyId;
        CreatedAt = createdAt;
        Status = status;
        Initiator = initiator;
        Source = source;
        Event = @event;
        Reason = reason;
        AgentName = agentName;
        AdminName = adminName;
    }

    
    public int Id { get; }
    public int BookingId { get; }
    public string UserId { get; }
    public ApiCallerTypes ApiCallerType { get; }
    public int? AgencyId { get; }
    public DateTimeOffset CreatedAt { get; }
    public BookingStatuses Status { get; }
    public BookingChangeInitiators Initiator { get; }
    public BookingChangeSources Source { get; }
    public BookingChangeEvents Event { get; }
    public string Reason { get; }
    public string? AgentName { get; }
    public string? AdminName { get; }
}