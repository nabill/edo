using System.Collections.Generic;

namespace HappyTravel.Edo.Api.Models.Mailing
{
    public class BookingConfirmationData : DataWithCompanyInfo
    {
        public string ReferenceCode { get; set; }

        public string AccommodationName { get; set; }

        public string BookingConfirmationPageUrl { get; set; }
    }
}
