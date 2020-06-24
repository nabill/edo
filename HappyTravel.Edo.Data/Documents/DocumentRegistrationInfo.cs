using System;

namespace HappyTravel.Edo.Data.Documents
{
    public readonly struct DocumentRegistrationInfo
    {
        public DocumentRegistrationInfo(string number, DateTime date)
        {
            Number = number;
            Date = date;
        }
        
        public string Number { get; }
        public DateTime Date { get; }
    }
}