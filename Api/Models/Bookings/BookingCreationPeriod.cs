using System;
using System.Text.Json.Serialization;

namespace HappyTravel.Edo.Api.Models.Bookings;

public readonly struct BookingCreationPeriod
{
    [JsonConstructor]
    public BookingCreationPeriod(DateTime startWith, DateTime endWith)
    {
        StartWith = startWith;
        EndWith = endWith;
    }


    public DateTime StartWith { get; }
    public DateTime EndWith { get; }

}