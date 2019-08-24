using System;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Availabilities;

namespace HappyTravel.Edo.Api.Services.Accommodations
{
    public readonly struct BookingAvailabilityInfo
    {
        public BookingAvailabilityInfo(in AvailabilityResponse availabilityResponse, in SlimAvailabilityResult result,
            in RichAgreement agreement)
        {
            AvailabilityResponse = availabilityResponse;
            Result = result;
            Agreement = agreement;
        }
        
        public AvailabilityResponse AvailabilityResponse { get; }
        public SlimAvailabilityResult Result { get; }
        public RichAgreement Agreement { get; }
        
        public bool Equals(BookingAvailabilityInfo other)
        {
            return (AvailabilityResponse, SelectedResult: Result, SelectedAgreement: Agreement)
                .Equals((other.AvailabilityResponse, other.Result, other.Agreement));
        }

        public override bool Equals(object obj)
        {
            return obj is BookingAvailabilityInfo other && Equals(other);
        }

        public override int GetHashCode() => throw new NotSupportedException();
    }
}