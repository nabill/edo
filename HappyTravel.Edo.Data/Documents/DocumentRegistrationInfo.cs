using System;

namespace HappyTravel.Edo.Data.Documents
{
    public class DocumentRegistrationInfo
    {
        // EF Constructor
        private DocumentRegistrationInfo()
        {
        }
        
        public DocumentRegistrationInfo(string number, DateTime date)
        {
            Number = number;
            Date = date;
        }
        
        public string Number { get; set; }
        public DateTime Date { get; set; }
    }
}