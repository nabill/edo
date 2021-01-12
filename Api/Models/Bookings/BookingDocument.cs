using System;

namespace HappyTravel.Edo.Api.Models.Bookings
{
    public readonly struct BookingDocument<TDocumentData>
    {
        public BookingDocument(string number, DateTime date, TDocumentData data)
        {
            Number = number;
            Date = date;
            Data = data;
        }
        
        public string Number { get; }
        public DateTime Date { get; }
        public TDocumentData Data { get; }
    }
}