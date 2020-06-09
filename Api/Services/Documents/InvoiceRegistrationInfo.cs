using System;

namespace HappyTravel.Edo.Api.Services.Documents
{
    public readonly struct InvoiceRegistrationInfo
    {
        public InvoiceRegistrationInfo(int id, DateTime date)
        {
            Id = id;
            Date = date;
        }
        
        public int Id { get; }
        public DateTime Date { get; }
    }
}