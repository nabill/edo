using System.ComponentModel.DataAnnotations;
using HappyTravel.SupplierOptionsClient.Models;

namespace HappyTravel.Edo.Api.Models.Management;

public readonly struct SetEnablementStateRequest
{
    public SetEnablementStateRequest(EnablementState state, string reason)
    {
        State = state;
        Reason = reason;
    }

    [Required]
    public EnablementState State { get; }
    
    [Required]
    public string Reason { get; }

}