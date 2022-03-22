using System;
using System.Collections.Generic;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.EdoContracts.Accommodations.Enums;

namespace HappyTravel.Edo.Api.Models.Bookings
{
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

        public readonly struct RoomInfo
        {
            public RoomInfo(string type, BoardBasisTypes boardBasis, string mealPlan,
                DateTime? deadlineDate, string contractDescription, List<Passenger> passengers,
                List<KeyValuePair<string, string>> remarks, string supplierRoomReferenceCode)
            {
                Type = type;
                BoardBasis = boardBasis;
                MealPlan = mealPlan;
                DeadlineDate = deadlineDate;
                ContractDescription = contractDescription;
                Passengers = passengers;
                Remarks = remarks;
                SupplierRoomReferenceCode = supplierRoomReferenceCode;
            }

            public string Type { get; }
            public BoardBasisTypes BoardBasis { get; }
            public string MealPlan { get; }
            public DateTime? DeadlineDate { get; }
            public string ContractDescription { get; }
            public List<Passenger> Passengers { get; }
            public List<KeyValuePair<string, string>> Remarks { get; }
            public string SupplierRoomReferenceCode { get; }
        }
    }
}