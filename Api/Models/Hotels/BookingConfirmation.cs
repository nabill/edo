using HappyTravel.Edo.Common.Enums;
using System;

namespace HappyTravel.Edo.Api.Models.Hotels
{
    public readonly struct BookingConfirmation
    {
        public string ReferenceCode { get; init; }
        public string ConfirmationCode { get; init; }
        public BookingConfirmationStatuses Status { get; init; }
        public string Initiator { get; init; }
        public DateTime CreatedAt { get; init; }
    }
}
