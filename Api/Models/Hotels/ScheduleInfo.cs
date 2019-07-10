using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Hotels
{
    public readonly struct ScheduleInfo
    {
        [JsonConstructor]
        public ScheduleInfo(string checkInTime, string portersStartTime, string portersEndTime, string roomServiceStartTime, string roomServiceEndTime)
        {
            CheckInTime = checkInTime;
            PortersStartTime = portersStartTime;
            PortersEndTime = portersEndTime;
            RoomServiceStartTime = roomServiceStartTime;
            RoomServiceEndTime = roomServiceEndTime;
        }


        public string CheckInTime { get; }

        public string PortersStartTime { get; }

        public string PortersEndTime { get; }

        public string RoomServiceStartTime { get; }

        public string RoomServiceEndTime { get; }
    }
}
