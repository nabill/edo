using System.ComponentModel.DataAnnotations;
using HappyTravel.Edo.Data.Agents;

namespace HappyTravel.Edo.Api.Models.Management;

public readonly struct ContractKindChangeRequest
{
    public ContractKindChangeRequest(ContractKind contractKind, string reason)
    {
        ContractKind = contractKind;
        Reason = reason;
    }
    
    
    /// <summary>
    /// Contract type
    /// </summary>
    [Required]
    public ContractKind ContractKind { get; }

    /// <summary>
    /// Verify reason.
    /// </summary>
    [Required]
    public string Reason { get; }
}