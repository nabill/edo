using System;

namespace HappyTravel.Edo.Api.Models.Reports;

public readonly struct AgenciesReportRow
{
    public string AgencyName { get; init; }
    public int AgencyId { get; init; }
    public string RootAgencyId { get; init; }
    public DateTime Created { get; init; }
    public decimal Balance { get; init; }
    public string City { get; init; }
    public string CountryCode { get; init; }
    public string ContractKind { get; init; }
    public string Suppliers { get; init; }
    public int GlobalMarkup { get; init; }
    public string IsActive { get; init; }
    public string IsContractLoaded { get; init; }
    public string AprMode { get; init; }
    public string PassedDeadlineOffersMode { get; init; }
    public string IsSupplierVisible { get; init; }
    public string IsDirectContractFlagVisible { get; init; }
    public int CustomDeadlineShift { get; init; }
    public string AvailableCurrencies { get; init; }
}