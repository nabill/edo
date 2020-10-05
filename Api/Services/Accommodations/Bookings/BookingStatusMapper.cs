using System;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.EdoContracts.Accommodations.Enums;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings
{
    public static class BookingStatusMapper
    {
        public static BookingStatuses ToStatus(this BookingStatusCodes code)
        {
            switch (code)
            {
                case BookingStatusCodes.InternalProcessing:
                    return BookingStatuses.InternalProcessing;
                case BookingStatusCodes.WaitingForResponse:
                    return BookingStatuses.Pending;
                case BookingStatusCodes.Pending:
                    return BookingStatuses.Pending;
                case BookingStatusCodes.Confirmed:
                    return BookingStatuses.Confirmed;
                case BookingStatusCodes.Cancelled:
                    return BookingStatuses.Cancelled;
                case BookingStatusCodes.Rejected:
                    return BookingStatuses.Rejected;
                case BookingStatusCodes.Invalid:
                    return BookingStatuses.Invalid;
                default:
                    throw new ArgumentException($"Invalid '{nameof(code)}': '{code}'");
            }
        }
    }
}