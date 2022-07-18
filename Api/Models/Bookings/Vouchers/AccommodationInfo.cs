using HappyTravel.Edo.Api.Models.Accommodations;
using SlimLocationInfo = HappyTravel.Edo.Api.Models.Accommodations.SlimLocationInfo;

namespace HappyTravel.Edo.Api.Models.Bookings.Vouchers;

public readonly struct AccommodationInfo
{
    public AccommodationInfo(string name, in SlimLocationInfo locationInfo, in ContactInfo contactInfo,
        string checkInTime, string checkOutTime)
    {
        ContactInfo = contactInfo;
        CheckInTime = checkInTime;
        CheckOutTime = checkOutTime;
        Location = locationInfo;
        Name = name;
    }


    public ContactInfo ContactInfo { get; }
    public string CheckInTime { get; }
    public string CheckOutTime { get; }
    public SlimLocationInfo Location { get; }
    public string Name { get; }
}
