using System.Text.Json;
using HappyTravel.Edo.Api.Models.Reports;

namespace HappyTravel.Edo.Api.Services.Reports.Converters;

public class PaymentLinkReportConverter : IConverter<PaymentLinkReportData, PaymentLinkReportRow>
{
    public PaymentLinkReportRow Convert(PaymentLinkReportData data) 
        => new()
        {
            Agent = (data.Agent != null ? data.Agent.FirstName + " " + data.Agent.LastName : "N/A"),
            Amount = data.Amount,
            Currency = data.Currency.ToString(),
            InvoiceNumber = !string.IsNullOrEmpty(data.InvoiceNumber) ? data.InvoiceNumber : "N/A",
            PaymentDate = data.PaymentDate is not null ? data.PaymentDate.ToString() : "N/A",
            PaymentProcessor = data.PaymentProcessor is not null ? data.PaymentProcessor.ToString() : "N/A",
            PaymentResponse = ParsePaymentResponse(data.PaymentResponse),
            ServiceType = data.ServiceType.ToString()
        };


    private string ParsePaymentResponse(string paymentResponse)
    {
        if (string.IsNullOrEmpty(paymentResponse))
            return "N/A";

        var json = JsonDocument.Parse(paymentResponse);
        return json.RootElement.GetProperty("status").GetString();
    }
}