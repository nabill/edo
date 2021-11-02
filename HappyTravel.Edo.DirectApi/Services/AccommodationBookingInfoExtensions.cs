using HappyTravel.Edo.Data.Bookings;
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
                agentInformation: bookingInfo.AgentInformation,
                paymentMethod: bookingInfo.PaymentMethod,
                tags: bookingInfo.Tags,
                isDirectContract: bookingInfo.IsDirectContract);
        }


        private static AccommodationBookingDetails FromEdoModel(this Edo.Api.Models.Bookings.AccommodationBookingDetails bookingDetails)
        {
            return new AccommodationBookingDetails(referenceCode: bookingDetails.ReferenceCode,
                agentReference: bookingDetails.AgentReference,
                status: bookingDetails.Status,
                numberOfNights: bookingDetails.NumberOfNights,
                checkInDate: bookingDetails.CheckInDate,
                checkOutDate: bookingDetails.CheckOutDate,
                location: bookingDetails.Location,
                contactInfo: bookingDetails.ContactInfo,
                accommodationId: bookingDetails.AccommodationId,
                accommodationName: bookingDetails.AccommodationName,
                accommodationInfo: bookingDetails.AccommodationInfo,
                deadlineDate: bookingDetails.DeadlineDate,
                roomDetails: bookingDetails.RoomDetails,
                numberOfPassengers: bookingDetails.NumberOfPassengers,
                cancellationPolicies: bookingDetails.CancellationPolicies,
                created: bookingDetails.Created,
                propertyOwnerConfirmationCode: bookingDetails.PropertyOwnerConfirmationCode,
                isAdvancePurchaseRate: );
        }
    }
}