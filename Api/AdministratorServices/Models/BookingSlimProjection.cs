using System;
using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.Money.Enums;

namespace HappyTravel.Edo.Api.AdministratorServices.Models;

public class BookingSlimProjection
{
    public int Id { get; set; }
    public string ReferenceCode { get; set; }
    public BookingPaymentStatuses PaymentStatus { get; set; }
    public int AgentId { get; set; }
    public int AgencyId { get; set; }
    public string AgentName { get; set; }
    public string AgencyName { get; set; }
    public string HtId { get; set; }
    public string AccommodationName { get; set; }
    public DateTimeOffset Created { get; set; }
    public DateTimeOffset CheckInDate { get; set; }
    public DateTimeOffset CheckOutDate { get; set; }
    public DateTimeOffset? DeadlineDate { get; set; }
    public decimal TotalPrice { get; set; }
    public Currencies Currency { get; set; }
    public BookingStatuses Status { get; set; }
    public PaymentTypes PaymentType { get; set; }
    public string Supplier { get; set; }
    public string SupplierCode { get; set; }
    public DateTimeOffset? CancellationDate { get; set; }
    public List<BookedRoom> Rooms { get; set; }
    public string MainPassengerName { get; set; }
}