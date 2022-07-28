using System.ComponentModel.DataAnnotations;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.Api.Models.Management;

public readonly struct ContractKindChangeRequest
{
    public ContractKindChangeRequest(int? agencyId, ContractKind contractKind,
            string reason, MoneyAmount? creditLimit)
    {
        AgencyId = agencyId;
        ContractKind = contractKind;
        Reason = reason;
        CreditLimit = creditLimit;
    }


    public ContractKindChangeRequest(int? agencyId, ContractKindChangeRequest request)
        : this(agencyId, request.ContractKind, request.Reason, request.CreditLimit)
    { }


    /// <summary>
    /// Agency Id
    /// </summary>
    public int? AgencyId { get; }

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

    /// <summary>
    /// Credit limit required when Contract type equals VirtualAccountOrCreditCardPayments.
    /// </summary>
    public MoneyAmount? CreditLimit { get; }
}