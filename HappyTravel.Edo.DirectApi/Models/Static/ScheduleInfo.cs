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
        ///     Check-in time for the accommodation
        /// </summary>
        public string CheckInTime { get; }

        /// <summary>
        ///     Check-out time for the accommodation
        /// </summary>
        public string CheckOutTime { get; }

        /// <summary>
        ///     Time when porters start working at the accommodation
        /// </summary>

        public string PortersStartTime { get; }

        /// <summary>
        ///     Time when porters stop working at the accommodation
        /// </summary>
        public string PortersEndTime { get; }

        /// <summary>
        ///     Time when room service starts at the accommodation
        /// </summary>
        public string RoomServiceStartTime { get; }

        /// <summary>
        ///     Time when room service stops at the accommodation
        /// </summary>
        public string RoomServiceEndTime { get; }
    }
}