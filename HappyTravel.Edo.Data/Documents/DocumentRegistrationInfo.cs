using System;

namespace HappyTravel.Edo.Data.Documents
{
    public class DocumentRegistrationInfo
    {
        // EF Constructor
        private DocumentRegistrationInfo()
        {
        }
        
        public DocumentRegistrationInfo(string number, DateTimeOffset date)
        {
            Number = number;
            Date = date;
        }
        
        public string Number { get; set; }
        public DateTimeOffset Date { get; set; }
    }
}