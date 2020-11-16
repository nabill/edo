using System;
using System.Collections.Generic;
using System.Linq;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Booking;
using HappyTravel.EdoContracts.Accommodations.Internals;
using HappyTravel.EdoContracts.General;
using HappyTravel.EdoContracts.General.Enums;
using HappyTravel.Money.Models;
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
            BookingAvailabilityInfo availabilityInfo,
            PaymentMethods paymentMethod,
            in AccommodationBookingRequest bookingRequest,
            string languageCode,
            Suppliers supplier,
            DateTime? deadlineDate, 
            DateTime checkInDate,
            DateTime checkOutDate)
        {
            var booking = new Booking
            {
                Created = created,
                ItineraryNumber = itineraryNumber,
                ReferenceCode = referenceCode,
                Status = BookingStatuses.InternalProcessing,
                PaymentMethod = paymentMethod,
                LanguageCode = languageCode,
                Supplier = supplier,
                PaymentStatus = BookingPaymentStatuses.NotPaid,
                DeadlineDate = deadlineDate,
                CheckInDate = checkInDate,
                CheckOutDate = checkOutDate
            };
            
            AddRequestInfo(bookingRequest);
            AddServiceDetails();
            AddAgentInfo();
            AddRooms(availabilityInfo.RoomContractSet.RoomContracts, bookingRequest.RoomDetails);

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
                booking.TotalPrice = price.NetTotal.Amount;
                booking.Currency = price.Currency;
                booking.Location = new AccommodationLocation(availabilityInfo.CountryName,
                    availabilityInfo.LocalityName,
                    availabilityInfo.ZoneName,
                    availabilityInfo.Address,
                    availabilityInfo.Coordinates);

                booking.AccommodationId = availabilityInfo.AccommodationId;
                booking.AccommodationName = availabilityInfo.AccommodationName;
            }

            void AddAgentInfo()
            {
                booking.AgentId = agentContext.AgentId;
                booking.AgencyId = agentContext.AgencyId;
                booking.CounterpartyId = agentContext.CounterpartyId;
            }
            
            void AddRooms(List<RoomContract> roomContracts, List<BookingRoomDetails> bookingRequestRoomDetails)
            {
                booking.Rooms = roomContracts
                    .Select((r, number) =>
                        new BookedRoom(r.Type,
                            r.IsExtraBedNeeded,
                            r.TotalPrice.NetTotal, 
                            r.BoardBasis,
                            r.MealPlan,
                            r.Deadline.Date,
                            r.ContractDescription,
                            r.Remarks,
                            r.Deadline,
                            GetCorrespondingPassengers(number),
                            string.Empty))
                    .ToList();
                
                List<Pax> GetCorrespondingPassengers(int number) => bookingRequestRoomDetails[number].Passengers;
            }
        }
    }
}
