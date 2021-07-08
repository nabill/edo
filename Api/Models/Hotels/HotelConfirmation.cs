using HappyTravel.Edo.Common.Enums;
using System;

namespace HappyTravel.Edo.Api.Models.Hotels
{
    public readonly struct HotelConfirmation
    {
        public string ReferenceCode { get; init; }
        public HotelConfirmationStatuses Status { get; init; }
        public string Initiator { get; init; }
        public DateTime CreatedAt { get; init; }
    }
}
