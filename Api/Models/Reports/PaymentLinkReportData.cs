using System;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Edo.Data.Management;
using HappyTravel.Money.Enums;

namespace HappyTravel.Edo.Api.Models.Reports;

public readonly struct PaymentLinkReportData
{
    public string InvoiceNumber { get; init; }
    public decimal Amount { get; init; }
    public Currencies Currency { get; init; }
    public DateTimeOffset? PaymentDate { get; init; }
    public DateTimeOffset Created { get; init; }
    public string PaymentResponse { get; init; }
    public PaymentProcessors? PaymentProcessor { get; init; }
    public ServiceTypes ServiceType { get; init; }
    public Administrator? Administrator { get; init; }
}