namespace HappyTravel.Edo.Api.Models.Mailing
{
    public class SlimVoucherData : DataWithCompanyInfo
    {
        public string AccommodationName { get; set; }
        public string ReferenceCode { get; set; }
        public string CheckInDate { get; set; }
        public string CheckOutDate { get; set; }
    }
}