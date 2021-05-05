using HappyTravel.Edo.Notifications.Enums;
using System;
using System.Text.Json;

namespace HappyTravel.Edo.Api.NotificationCenter.Models
{
    public readonly struct SlimNotification
    {
        public int Id { get; init; }
        public int UserId { get; init; }
        public int? AgencyId { get; init; }
        public JsonDocument Message { get; init; }
        public NotificationTypes Type { get; init; }
        public SendingStatuses SendingStatus { get; init; }
        public DateTime Created { get; init; }
    }
}