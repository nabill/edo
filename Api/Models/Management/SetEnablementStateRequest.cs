using System.ComponentModel.DataAnnotations;
using HappyTravel.SupplierOptionsClient.Models;

namespace HappyTravel.Edo.Api.Models.Management;

public readonly struct SetEnableStateRequest
{
    public SetEnableStateRequest(EnableState state, string reason)
    {
        State = state;
        Reason = reason;
    }

    [Required]
    public EnableState State { get; }
    
    [Required]
    public string Reason { get; }

}