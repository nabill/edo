using System;
using System.Collections.Generic;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Common.Enums.AgencySettings;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public static class RoomContractSetSettingsChecker
    {
        public static bool IsDisplayAllowed(RoomContractSet roomSet, DateTimeOffset checkInDate, AccommodationBookingSettings settings,
            IDateTimeProvider dateTimeProvider)
        {
            return IsAllowed(roomSet,
                checkInDate,
                settings,
                dateTimeProvider,
                new HashSet<AprMode> {AprMode.Hide},
                new HashSet<PassedDeadlineOffersMode> {PassedDeadlineOffersMode.Hide});
        }


        public static bool IsEvaluationAllowed(RoomContractSet roomSet, DateTimeOffset checkInDate, AccommodationBookingSettings settings,
            IDateTimeProvider dateTimeProvider)
        {
            return IsAllowed(roomSet,
                checkInDate,
                settings,
                dateTimeProvider,
                new HashSet<AprMode> {AprMode.Hide, AprMode.DisplayOnly},
                new HashSet<PassedDeadlineOffersMode> {PassedDeadlineOffersMode.Hide, PassedDeadlineOffersMode.DisplayOnly});
        }


        private static bool IsAllowed(RoomContractSet roomSet, DateTimeOffset checkInDate, AccommodationBookingSettings settings, IDateTimeProvider dateTimeProvider,
            HashSet<AprMode> aprModesToDisallow, HashSet<PassedDeadlineOffersMode> deadlineModesToDisallow)
        {
            if (roomSet.IsAdvancePurchaseRate && aprModesToDisallow.Contains(settings.AprMode))
                return false;

            if (deadlineModesToDisallow.Contains(settings.PassedDeadlineOffersMode))
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