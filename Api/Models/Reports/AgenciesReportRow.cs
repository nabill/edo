using HappyTravel.Edo.Data.Agents;
using HappyTravel.Edo.Data.Markup;
using HappyTravel.Edo.Data.Payments;

namespace HappyTravel.Edo.Api.Models.Reports;

public readonly struct AgenciesReportRow
{
    public string AgencyName { get; init; }
    public int AgencyId { get; init; }
    public string RootAgencyId { get; init; }
    public decimal Balance { get; init; }
    public string City { get; init; }
    public string CountryCode { get; init; }
    public string ContractKind { get; init; }
    public string Suppliers { get; init; }
    public int GlobalMarkup { get; init; }
    public string IsActive { get; init; }
    public string IsContractLoaded { get; init; }
}