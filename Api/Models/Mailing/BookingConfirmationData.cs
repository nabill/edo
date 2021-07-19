namespace HappyTravel.Edo.Api.Models.Mailing
{
    public class BookingConfirmationData : DataWithCompanyInfo
    {
        public string ReferenceCode { get; set; }
        public string AccommodationName { get; set; }
        public string MainPassengerName { get; set; }
        public string RoomType { get; set; }
        public string CheckInDate { get; set; }
        public string CheckOutDate { get; set; }
        public string PromoCodeAndRate { get; set; }
        public string MealPlan { get; set; }
        public int NumberOfPassengers { get; set; }
        public string Bedding { get; set; }
        public string BookingConfirmationPageUrl { get; set; }
    }
}
