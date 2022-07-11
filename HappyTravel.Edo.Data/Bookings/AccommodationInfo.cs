namespace HappyTravel.Edo.Data.Bookings
{
    public class AccommodationInfo
    {
        // EF constructor
        private AccommodationInfo() { }


        public AccommodationInfo(ImageInfo? photo, ContactInfo? contactInfo)
        {
            Photo = photo;
            ContactInfo = contactInfo;
        }
        
        public ImageInfo? Photo { get; set; }
        
        public ContactInfo? ContactInfo { get; set; }
    }
}
