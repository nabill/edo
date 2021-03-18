using System;

namespace HappyTravel.Edo.NotificationCenter.Models
{
    public readonly struct NotificationSlim
    {
        public int Id { get; init; }
        public int UserId { get; init; }
        public string Message { get; init; }
        public DateTime Created { get; init; }
        public bool IsRead { get; init; }
    }
}