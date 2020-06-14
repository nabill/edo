using System;

namespace HappyTravel.Edo.Data.Documents
{
    public readonly struct DocumentRegistrationInfo
    {
        public DocumentRegistrationInfo(int id, DateTime date)
        {
            Id = id;
            Date = date;
        }
        
        public int Id { get; }
        public DateTime Date { get; }
    }
}