using System.ComponentModel;

namespace HappyTravel.Edo.Common.Enums
{
    public enum OfflineDeadlineNotifications
    {
        [Description("After booking confirmed")]
        AfterBookingConfirmed = 0,

        [Description("Fifteen days before")]
        FifteenDays = 1,

        [Description("Seven days before")]
        SevenDays = 2,

        [Description("Three days before")]
        ThreeDays = 4,

        [Description("Two days before")]
        TwoDays = 8,

        [Description("One day before")]
        OneDay = 16,
    }
}