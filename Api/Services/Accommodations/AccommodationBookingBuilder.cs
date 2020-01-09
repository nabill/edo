using System;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Models.Customers;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Booking;
using HappyTravel.EdoContracts.Accommodations;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Services.Accommodations
{
    internal class AccommodationBookingBuilder
    {
        public AccommodationBookingBuilder()
        {
            _booking = new Booking {ServiceType = ServiceTypes.HTL};
        }


        public AccommodationBookingBuilder AddRequestInfo(in AccommodationBookingRequest bookingRequest)
        {
            _booking.AgentReference = bookingRequest.AgentReference;
            _booking.Nationality = bookingRequest.Nationality;
            _booking.Residency = bookingRequest.Residency;
            _booking.MainPassengerName = bookingRequest.MainPassengerName;
            _booking.PaymentMethod = bookingRequest.PaymentMethod;
            return this;
        }


        public AccommodationBookingBuilder AddConfirmationDetails(in BookingDetails confirmedBooking)
        {
            _booking.Status = confirmedBooking.Status;
            _booking.BookingDate = confirmedBooking.CheckInDate;
            _booking.ReferenceCode = confirmedBooking.ReferenceCode;
            _booking.BookingDetails = JsonConvert.SerializeObject(confirmedBooking);
            return this;
        }


        public AccommodationBookingBuilder AddServiceDetails(in BookingAvailabilityInfo availabilityInfo)
        {
            _booking.ServiceDetails = JsonConvert.SerializeObject(availabilityInfo);
            return this;
        }


        public AccommodationBookingBuilder AddTags(string itn, string referenceNumber)
        {
            _booking.ItineraryNumber = itn;
            _booking.ReferenceCode = referenceNumber;
            return this;
        }


        public AccommodationBookingBuilder AddCustomerInfo(CustomerInfo customerInfo)
        {
            _booking.CustomerId = customerInfo.CustomerId;
            _booking.CompanyId = customerInfo.CompanyId;
            return this;
        }


        public AccommodationBookingBuilder AddCreationDate(DateTime date)
        {
            _booking.Created = date;
            return this;
        }


        public Booking Build() => _booking;

        private readonly Booking _booking;
    }
}