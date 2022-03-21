using System;
using System.Collections.Generic;

namespace HappyTravel.Edo.Api.Models.Bookings.Vouchers;

public readonly struct BookingVoucherData
{
    public BookingVoucherData(string agentName, int bookingId, in AccommodationInfo accommodation, int nightCount,
        in DateTime checkInDate, in DateTime checkOutDate, DateTime? deadlineDate, string mainPassengerName, string referenceCode,
        string supplierReferenceCode, string propertyOwnerConfirmationCode, string bannerUrl, string logoUrl, List<RoomInfo> roomDetails,
        List<KeyValuePair<string, string>> specialValues)
    {
        AgentName = agentName;
        Accommodation = accommodation;
        NightCount = nightCount;
        BookingId = bookingId;
        CheckInDate = checkInDate;
        CheckOutDate = checkOutDate;
        DeadlineDate = deadlineDate;
        MainPassengerName = mainPassengerName;
        ReferenceCode = referenceCode;
        SupplierReferenceCode = supplierReferenceCode;
        PropertyOwnerConfirmationCode = propertyOwnerConfirmationCode;
        BannerUrl = bannerUrl;
        LogoUrl = logoUrl;
        RoomDetails = roomDetails;
        SpecialValues = specialValues;
    }


    public int BookingId { get; }
    public string AgentName { get; }
    public AccommodationInfo Accommodation { get; }
    public int NightCount { get; }
    public DateTime CheckInDate { get; }
    public DateTime CheckOutDate { get; }
    public DateTime? DeadlineDate { get; }
    public string MainPassengerName { get; }
    public string ReferenceCode { get; }
    public string SupplierReferenceCode { get; }
    public string PropertyOwnerConfirmationCode { get; }
    public string BannerUrl { get; }
    public string LogoUrl { get; }
    public List<RoomInfo> RoomDetails { get; }
    public List<KeyValuePair<string, string>> SpecialValues { get; }
}
