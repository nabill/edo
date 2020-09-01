using System;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Booking;
using HappyTravel.EdoContracts.Accommodations.Enums;
using HappyTravel.EdoContracts.General.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings
{
    public static class BookingFactory
    {
        public static Booking Create(
            DateTime created,
            AgentContext agentContext,
            string itineraryNumber,
            string referenceCode,
            BookingStatusCodes status,
            BookingAvailabilityInfo availabilityInfo,
            PaymentMethods paymentMethod,
            in AccommodationBookingRequest bookingRequest,
            string languageCode,
            DataProviders dataProvider,
            BookingPaymentStatuses paymentStatus)
        {
            var booking = new Booking
            {
                Created = created,
                ItineraryNumber = itineraryNumber,
                ReferenceCode = referenceCode,
                Status = status,
                PaymentMethod = paymentMethod,
                LanguageCode = languageCode,
                DataProvider = dataProvider,
                PaymentStatus = paymentStatus
            };

            AddRequestInfo(bookingRequest);
            AddServiceDetails();
            AddAgentInfo();

            return booking;


            void AddRequestInfo(in AccommodationBookingRequest bookingRequestInternal)
            {
                booking.Nationality = bookingRequestInternal.Nationality;
                booking.Residency = bookingRequestInternal.Residency;
                booking.MainPassengerName = bookingRequestInternal.MainPassengerName;
                booking.BookingRequest = JsonConvert.SerializeObject(bookingRequestInternal);
            }

            void AddServiceDetails()
            {
                var price = availabilityInfo.RoomContractSet.Price;
                booking.TotalPrice = price.NetTotal;
                booking.Currency = price.Currency;
                booking.Location = new AccommodationLocation(availabilityInfo.CountryName,
                    availabilityInfo.LocalityName,
                    availabilityInfo.ZoneName,
                    availabilityInfo.Address,
                    availabilityInfo.Coordinates);

                booking.AddRooms(availabilityInfo.RoomContractSet.RoomContracts);

                booking.AccommodationId = availabilityInfo.AccommodationId;
                booking.AccommodationName = availabilityInfo.AccommodationName;
            }

            void AddAgentInfo()
            {
                booking.AgentId = agentContext.AgentId;
                booking.AgencyId = agentContext.AgencyId;
                booking.CounterpartyId = agentContext.CounterpartyId;
            }
        }


        public static Booking CreateInitial(DateTime created) =>
            new Booking
            {
                Created = created,
                Status = BookingStatusCodes.InternalProcessing,
                PaymentStatus = BookingPaymentStatuses.NotPaid
            };
    }
}
