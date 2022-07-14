using System;
using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.EdoContracts.Accommodations.Enums;
using HappyTravel.Money.Enums;

namespace HappyTravel.Edo.Data.Bookings
{
    public class Booking : IEntity
    {
        public int Id { get; set; }
        public int AgentId { get; set; }
        public int AgencyId { get; set; }
        public string? HtId { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset CheckInDate { get; set; }
        public DateTimeOffset CheckOutDate { get; set; }
        public DateTimeOffset? DeadlineDate { get; set; }
        public decimal NetPrice { get; set; }
        public decimal Commission { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal CreditCardPrice { get; set; }
        public Currencies Currency { get; set; }
        public string? SupplierReferenceCode { get; set; }
        public string ReferenceCode { get; set; } = string.Empty;
        public string? ClientReferenceCode { get; set; }
        public BookingStatuses Status { get; set; }
        public BookingPaymentStatuses PaymentStatus { get; set; }
        public DateTimeOffset? ConfirmationDate { get; set; }
        public string? Nationality { get; set; }
        public string? Residency { get; set; }
        public string ItineraryNumber { get; set; } = string.Empty;
        public string? MainPassengerName { get; set; }
        public PaymentTypes PaymentType { get; set; }
        public string LanguageCode { get; set; } = string.Empty;
        public string? SupplierCode { get; set; }
        public BookingUpdateModes UpdateMode { get; set; }
        public List<BookedRoom> Rooms { get; set; } = new();
        public string AccommodationId { get; set; } = string.Empty;
        public string AccommodationName { get; set; } = string.Empty;
        public AccommodationInfo? AccommodationInfo { get; set; }
        public AccommodationLocation Location { get; set; } = null!;
        public List<string>? Tags { get; set; }
        public bool IsDirectContract { get; set; }
        public List<CancellationPolicy> CancellationPolicies { get; set; } = null!;
        public string? PropertyOwnerConfirmationCode { get; set; }
        public DateTimeOffset? Cancelled { get; set; }
        public bool IsAdvancePurchaseRate { get; set; }
        public bool IsPackage { get; set; }
        public List<KeyValuePair<string, string>>? SpecialValues { get; set; }
        public DateTimeOffset? DeadlineNotificationSent { get; set; }
        public OfflineDeadlineNotifications? OfflineDeadlineNotificationsSent { get; set; }
    }
}