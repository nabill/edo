using HappyTravel.Edo.Data.Bookings;
using HappyTravel.EdoContracts.Accommodations.Enums;
using System;
using System.Collections.Generic;

namespace HappyTravel.Edo.Api.Models.Bookings.Vouchers;

public readonly struct RoomInfo
{
    public RoomInfo(string type, BoardBasisTypes boardBasis, string mealPlan,
        DateTime? deadlineDate, string contractDescription, List<Passenger> passengers,
        List<KeyValuePair<string, string>> remarks, string supplierRoomReferenceCode,
        string occupancy = "")
    {
        Type = type;
        BoardBasis = boardBasis;
        MealPlan = mealPlan;
        DeadlineDate = deadlineDate;
        ContractDescription = contractDescription;
        Passengers = passengers;
        Remarks = remarks;
        SupplierRoomReferenceCode = supplierRoomReferenceCode;
        Occupancy = occupancy;
    }


    public RoomInfo(string occupancy, RoomInfo roomInfo)
        : this(roomInfo.Type, roomInfo.BoardBasis, roomInfo.MealPlan, roomInfo.DeadlineDate, roomInfo.ContractDescription,
            roomInfo.Passengers, roomInfo.Remarks, roomInfo.SupplierRoomReferenceCode, occupancy)
    { }


    public string Type { get; }
    public BoardBasisTypes BoardBasis { get; }
    public string MealPlan { get; }
    public DateTime? DeadlineDate { get; }
    public string ContractDescription { get; }
    public List<Passenger> Passengers { get; }
    public List<KeyValuePair<string, string>> Remarks { get; }
    public string SupplierRoomReferenceCode { get; }
    public string Occupancy { get; }
}
