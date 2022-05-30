using HappyTravel.Edo.Data.Agents;

namespace HappyTravel.Edo.Api.Models.Management.AuditEvents;

public readonly struct ChangeContractKindData
{
    public ChangeContractKindData(int agencyId, ContractKind contractKind, string reason)
    {
        AgencyId = agencyId;
        ContractKind = contractKind;
        Reason = reason;
    }
    

    public int AgencyId { get; }
    public ContractKind ContractKind { get; }
    public string Reason { get; }
}