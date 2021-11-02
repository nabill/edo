using HappyTravel.Edo.DirectApi.Models;

namespace HappyTravel.Edo.DirectApi.Services
{
    public static class AccommodationBookingInfoExtensions
    {
        public static AccommodationBookingInfo FromEdoModel(this Edo.Api.Models.Bookings.AccommodationBookingInfo bookingInfo)
        {
            return new AccommodationBookingInfo(bookingId: bookingInfo.BookingId,
                bookingDetails: bookingInfo.BookingDetails.FromEdoModel(),
                counterpartyId: bookingInfo.CounterpartyId,
                paymentStatus: bookingInfo.PaymentStatus,
                totalPrice: bookingInfo.TotalPrice,
                supplier: bookingInfo.Supplier,
                agentInformation: bookingInfo.AgentInformation.FromEdoModels(),
                paymentMethod: bookingInfo.PaymentMethod,
                tags: bookingInfo.Tags,
                isDirectContract: bookingInfo.IsDirectContract);
        }


        private static AccommodationBookingDetails FromEdoModel(this Edo.Api.Models.Bookings.AccommodationBookingDetails bookingDetails)
        {
            return new AccommodationBookingDetails(referenceCode: bookingDetails.ClientReferenceCode,
                supplierReferenceCode: bookingDetails.ReferenceCode,
                status: bookingDetails.Status,
                numberOfNights: bookingDetails.NumberOfNights,
                checkInDate: bookingDetails.CheckInDate,
                checkOutDate: bookingDetails.CheckOutDate,
                location: bookingDetails.Location,
                contactInfo: bookingDetails.ContactInfo.FromEdoModels(),
                accommodationId: bookingDetails.AccommodationId,
                accommodationName: bookingDetails.AccommodationName,
                accommodationInfo: bookingDetails.AccommodationInfo,
                deadlineDate: bookingDetails.DeadlineDate,
                roomDetails: bookingDetails.RoomDetails,
                numberOfPassengers: bookingDetails.NumberOfPassengers,
                cancellationPolicies: bookingDetails.CancellationPolicies,
                created: bookingDetails.Created,
                propertyOwnerConfirmationCode: bookingDetails.PropertyOwnerConfirmationCode);
        }


        private static AccommodationBookingInfo.BookingAgentInformation FromEdoModels(this Api.Models.Bookings.AccommodationBookingInfo.BookingAgentInformation agentInformation)
        {
            return new AccommodationBookingInfo.BookingAgentInformation(agentName: agentInformation.AgentName,
                agencyName: agentInformation.AgencyName,
                counterpartyName: agentInformation.CounterpartyName,
                agentEmail: agentInformation.AgentEmail);
        }


        private static ContactInfo FromEdoModels(this Api.Models.Accommodations.ContactInfo contactInfo)
        {
            return new ContactInfo(emails: contactInfo.Emails, 
                phones: contactInfo.Phones, 
                webSites: contactInfo.WebSites, 
                faxes: contactInfo.Faxes);
        }
    }
}