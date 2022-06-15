using System;

namespace HappyTravel.Edo.Api.Models.Reports;

public struct PaymentLinkReportRow
{
    public string InvoiceNumber { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; }
    public string PaymentDate { get; init; }
    public string PaymentResponse { get; init; }
    public string PaymentProcessor { get; init; }
    public string ServiceType { get; init; }
    public string Agent { get; init; }
}