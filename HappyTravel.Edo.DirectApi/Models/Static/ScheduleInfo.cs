using System.Text.Json.Serialization;

namespace HappyTravel.Edo.DirectApi.Models.Static
{
    public readonly struct ScheduleInfo
    {
        [JsonConstructor]
        public ScheduleInfo(string checkInTime, string checkOutTime, string? portersStartTime = null, string? portersEndTime = null,
            string? roomServiceStartTime = null, string? roomServiceEndTime = null)
        {
            CheckInTime = checkInTime;
            CheckOutTime = checkOutTime;
            PortersStartTime = portersStartTime ?? string.Empty;
            PortersEndTime = portersEndTime ?? string.Empty;
            RoomServiceStartTime = roomServiceStartTime ?? string.Empty;
            RoomServiceEndTime = roomServiceEndTime ?? string.Empty;
        }


        /// <summary>
        ///     The check-in time in an accommodation.
        /// </summary>
        public string CheckInTime { get; }

        /// <summary>
        ///     The check-out time in an accommodation.
        /// </summary>
        public string CheckOutTime { get; }

        /// <summary>
        ///     The time when porters start working in an accommodation.
        /// </summary>

        public string PortersStartTime { get; }

        /// <summary>
        ///     The time when porters end working in an accommodation.
        /// </summary>
        public string PortersEndTime { get; }

        /// <summary>
        ///     The time when a room service start working in an accommodation.
        /// </summary>
        public string RoomServiceStartTime { get; }

        /// <summary>
        ///     The time when a room service end working in an accommodation.
        /// </summary>
        public string RoomServiceEndTime { get; }
    }
}