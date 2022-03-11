using System.ComponentModel.DataAnnotations;

namespace HappyTravel.Edo.Api.Models.Markups.Agent;

public readonly struct SetAgentMarkupRequest
{
    [Range(0.1d, 100d)]
    public decimal Percent { get; init; }
}