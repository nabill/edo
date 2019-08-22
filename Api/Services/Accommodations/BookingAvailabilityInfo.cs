using System;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Availabilities;

namespace HappyTravel.Edo.Api.Services.Accommodations
{
    public readonly struct BookingAvailabilityInfo
    {
        public BookingAvailabilityInfo(in AvailabilityResponse availabilityResponse, in SlimAvailabilityResult selectedResult,
            in RichAgreement selectedAgreement)
        {
            AvailabilityResponse = availabilityResponse;
            SelectedResult = selectedResult;
            SelectedAgreement = selectedAgreement;
        }
        
        public AvailabilityResponse AvailabilityResponse { get; }
        public SlimAvailabilityResult SelectedResult { get; }
        public RichAgreement SelectedAgreement { get; }
        
        public bool Equals(BookingAvailabilityInfo other)
        {
            return (AvailabilityResponse, SelectedResult, SelectedAgreement)
                .Equals((other.AvailabilityResponse, other.SelectedResult, other.SelectedAgreement));
        }

        public override bool Equals(object obj)
        {
            return obj is BookingAvailabilityInfo other && Equals(other);
        }

        public override int GetHashCode() => throw new NotSupportedException();
    }
}