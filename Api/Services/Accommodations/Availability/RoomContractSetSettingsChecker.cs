using System;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Common.Enums.AgencySettings;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public static class RoomContractSetSettingsChecker
    {
        public static bool IsAllowed(RoomContractSet roomSet, DateTime checkInDate, AccommodationBookingSettings settings, IDateTimeProvider dateTimeProvider)
        {
            if (settings.AprMode == AprMode.Hide && roomSet.IsAdvancePurchaseRate)
                return false;

            if (settings.PassedDeadlineOffersMode == PassedDeadlineOffersMode.Hide)
            {
                var tomorrow = dateTimeProvider.UtcTomorrow();
                if (checkInDate <= tomorrow)
                    return false;

                var deadlineDate = roomSet.Deadline.Date;
                if (deadlineDate.HasValue && deadlineDate.Value.Date <= tomorrow)
                    return false;
            }

            return true;
        }
    }
}