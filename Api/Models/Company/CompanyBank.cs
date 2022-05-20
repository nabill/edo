using System;

namespace HappyTravel.Edo.Api.Models.Company;

public record CompanyBankInfo
{
    public int Id { get; init; } = default;
    public string Name { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string RoutingCode { get; init; } = string.Empty;
    public string SwiftCode { get; init; } = string.Empty;
    public DateTimeOffset Created { get; init; }
    public DateTimeOffset Modified { get; init; }
}
