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
        string adultOccupancy = "", string childOccupancy = "")
    {
        Type = type;
        BoardBasis = boardBasis;
        MealPlan = mealPlan;
        DeadlineDate = deadlineDate;
        ContractDescription = contractDescription;
        Passengers = passengers;
        Remarks = remarks;
        SupplierRoomReferenceCode = supplierRoomReferenceCode;
        AdultOccupancy = adultOccupancy;
        ChildOccupancy = childOccupancy;
    }


    public string Type { get; }
    public BoardBasisTypes BoardBasis { get; }
    public string MealPlan { get; }
    public DateTime? DeadlineDate { get; }
    public string ContractDescription { get; }
    public List<Passenger> Passengers { get; }
    public List<KeyValuePair<string, string>> Remarks { get; }
    public string SupplierRoomReferenceCode { get; }
    public string AdultOccupancy { get; }
    public string ChildOccupancy { get; }
}
