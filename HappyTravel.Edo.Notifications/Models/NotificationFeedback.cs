using HappyTravel.Edo.Notifications.Enums;
using System;

namespace HappyTravel.Edo.Notifications.Models
{
    public readonly struct NotificationFeedback
    {
        public int MessageId { get; init; }
        public SendingStatuses SendingStatus { get; init; }
        public DateTime StatusChangeTime { get; init; }
    }
}