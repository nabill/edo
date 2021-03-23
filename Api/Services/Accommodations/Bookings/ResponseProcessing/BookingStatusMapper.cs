using System;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.EdoContracts.Accommodations.Enums;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.ResponseProcessing
{
    public static class BookingStatusMapper
    {
        public static BookingStatuses ToInternalStatus(this BookingStatusCodes code)
        {
            return code switch
            {
                BookingStatusCodes.InternalProcessing => BookingStatuses.WaitingForResponse,
                BookingStatusCodes.WaitingForResponse => BookingStatuses.Pending,
                BookingStatusCodes.Pending => BookingStatuses.Pending,
                BookingStatusCodes.Confirmed => BookingStatuses.Confirmed,
                BookingStatusCodes.Cancelled => BookingStatuses.Cancelled,
                BookingStatusCodes.Rejected => BookingStatuses.Rejected,
                BookingStatusCodes.Invalid => BookingStatuses.Invalid,
                _ => throw new ArgumentException($"Invalid '{nameof(code)}': '{code}'")
            };
        }
    }
}