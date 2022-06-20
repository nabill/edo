using System;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Edo.Data.Markup;
using HappyTravel.Edo.Data.Payments;

namespace HappyTravel.Edo.Api.Models.Reports;

public readonly struct AgenciesReportData
{
    public string AgencyName { get; init; }
    public int AgencyId { get; init; }
    public int? RootAgencyId { get; init; }
    public AgencyAccount? AgencyAccount { get; init; }
    public DateTimeOffset Created { get; init; }
    public string City { get; init; }
    public string CountryCode { get; init; }
    public ContractKind? ContractKind { get; init; }
    public AgencySystemSettings? AgencySystemSettings { get; init; }
    public MarkupPolicy? GlobalMarkup { get; init; }
    public bool IsActive { get; init; }
    public bool IsContractLoaded { get; init; }
}