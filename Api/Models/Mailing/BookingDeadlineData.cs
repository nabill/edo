namespace HappyTravel.Edo.Api.Models.Mailing
{
    public class BookingDeadlineData : DataWithCompanyInfo
    {
        public int BookingId { get; set; }
        public string RoomDescriptions { get; set; }
        public string Passengers { get; set; }
        public string ReferenceCode { get; set; }
        public string CheckInDate { get; set; }
        public string CheckOutDate { get; set; }
        public string Deadline { get; set; }
    }
}