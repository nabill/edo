using HappyTravel.Edo.Data.Agents;
using HappyTravel.Edo.Data.Markup;
using HappyTravel.Edo.Data.Payments;

namespace HappyTravel.Edo.Api.Models.Reports;

public readonly struct AgenciesReportRow
{
    public string AgencyName { get; init; }
    public int AgencyId { get; init; }
    public int? RootAgencyId { get; init; }
    public string Balance { get; init; }
    public string City { get; init; }
    public string CountryCode { get; init; }
    public string ContractKind { get; init; }
    public string Suppliers { get; init; }
    public int Markup { get; init; }
    public MarkupPolicy? GlobalMarkup { get; init; }
    public bool IsActive { get; init; }
    public bool IsContractLoaded { get; init; }
}