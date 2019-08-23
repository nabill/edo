using System;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Booking;
using HappyTravel.Edo.Data.Customers;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Services.Accommodations
{
    internal static class AccommodationBookingBuilder
    {
        public static AccommodationBooking AddRequestInfo(this AccommodationBooking booking,
            AccommodationBookingRequest bookingRequest)
        {
            booking.AgentReference = bookingRequest.AgentReference;
            booking.Nationality = bookingRequest.Nationality;
            booking.Residency = bookingRequest.Residency;
            booking.RoomDetails = JsonConvert.SerializeObject(bookingRequest.RoomDetails);
            booking.MainPassengerName = bookingRequest.MainPassengerName;
            booking.PaymentMethod = bookingRequest.PaymentMethod;
            return booking;
        }
        
        public static AccommodationBooking AddConfirmedDetails(this AccommodationBooking booking,
            AccommodationBookingDetails confirmedBooking)
        {
            booking.Deadline = confirmedBooking.Deadline;
            booking.Status = confirmedBooking.Status;
            booking.CheckInDate = confirmedBooking.CheckInDate;
            booking.CheckOutDate = confirmedBooking.CheckOutDate;
            booking.ContractTypeId = confirmedBooking.ContractTypeId;
            booking.ReferenceCode = confirmedBooking.ReferenceCode;
            return booking;
        }
        
        public static AccommodationBooking AddConditions(this AccommodationBooking booking,
            BookingAvailabilityInfo selectedAvailabilityInfo)
        {
            booking.AccommodationId = selectedAvailabilityInfo.SelectedResult.AccommodationDetails.Id;
            booking.Service = selectedAvailabilityInfo.SelectedResult.AccommodationDetails.Name;
            booking.TariffCode = selectedAvailabilityInfo.SelectedAgreement.TariffCode;
            booking.RateBasis = selectedAvailabilityInfo.SelectedAgreement.BoardBasis;
            booking.PriceCurrency = Enum.Parse<Currencies>(selectedAvailabilityInfo.SelectedAgreement.CurrencyCode); 
            booking.CountryCode = selectedAvailabilityInfo.SelectedResult.AccommodationDetails.Location.CountryCode;
            booking.CityCode = selectedAvailabilityInfo.SelectedResult.AccommodationDetails.Location.CityCode;
            booking.Features = selectedAvailabilityInfo.SelectedAgreement.Remarks;
            return booking;
        }

        public static AccommodationBooking AddReferences(this AccommodationBooking booking, long itn, string referenceNumber)
        {
            booking.ItineraryNumber = itn;
            booking.ReferenceCode = referenceNumber;
            return booking;
        }

        public static AccommodationBooking AddCustomerInformation(this AccommodationBooking booking, Customer customer,
            int companyId)
        {
            booking.CustomerId = customer.Id;
            booking.CompanyId = companyId;
            return booking;
        }

        public static AccommodationBooking AddDate(this AccommodationBooking booking, IDateTimeProvider dateTimeProvider)
        {
            booking.BookingDate = dateTimeProvider.UtcNow();
            return booking;
        }
    }
}