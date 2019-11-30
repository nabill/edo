using System.Collections.Generic;

namespace HappyTravel.Edo.Api.Models.Payments
{
    public readonly struct CompletePaymentsModel
    {
        public CompletePaymentsModel(List<int> bookingIds)
        {
            BookingIds = bookingIds ?? new List<int>(0);
        }

        /// <summary>
        ///     List of booking ids that should be completed
        /// </summary>
        public List<int> BookingIds { get; }
    }
}