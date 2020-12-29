using System;
using System.Collections.Generic;
using HappyTravel.EdoContracts.Accommodations.Enums;

namespace HappyTravel.Edo.Api.Models.Mailing
{
    public class PaymentReceiptData : DataWithCompanyInfo
    {
        public string Amount { get; set; }
        public string CustomerName { get; set; }
        public string Date { get; set; }
        public string Method { get; set; }
        public string Number { get; set; }
        public string InvoiceNumber { get; set; }
        public string InvoiceDate { get; set; }
        public string ReferenceCode { get; set; }
        public string AccommodationName { get; set; }
        public string CheckInDate { get; set; }
        public string CheckOutDate { get; set; }
        public string DeadlineDate { get; set; }
        public List<RoomDetails> RoomTypes { get; set; }
        public Buyer BuyerInformation { get; set; }

        
        public class RoomDetails
        {
            public DateTime? DeadlineDate { get; set; }
            public RoomTypes RoomType { get; set; }
        }


        public class Buyer
        {
            public string Address { get; set; }
            public string ContactPhone { get; set; }
            public string Email { get; set; }
            public string Name { get; set; }
        }
    }
}