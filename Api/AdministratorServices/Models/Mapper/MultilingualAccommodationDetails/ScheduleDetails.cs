namespace HappyTravel.Edo.Api.AdministratorServices.Models.Mapper.MultilingualAccommodationDetails;

public struct ScheduleDetails
{
    public ScheduleDetails(
        string checkInTime,
        string checkOutTime,
        string portersStartTime = null,
        string portersEndTime = null,
        string roomServiceStartTime = null,
        string roomServiceEndTime = null)
    {
        CheckInTime = checkInTime;
        CheckOutTime = checkOutTime;
        PortersStartTime = portersStartTime ?? string.Empty;
        PortersEndTime = portersEndTime ?? string.Empty;
        RoomServiceStartTime = roomServiceStartTime ?? string.Empty;
        RoomServiceEndTime = roomServiceEndTime ?? string.Empty;
    }

    
    public string CheckInTime { get; }
    public string CheckOutTime { get; }
    public string PortersStartTime { get; }
    public string PortersEndTime { get; }
    public string RoomServiceStartTime { get; }
    public string RoomServiceEndTime { get; }
}