using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Models.PropertyOwners
{
    public readonly struct SlimBookingConfirmation
    {
        public string ReferenceCode { get; init; }
        public string ConfirmationCode { get; init; }
        public BookingStatuses Status { get; init; }
    }
}
