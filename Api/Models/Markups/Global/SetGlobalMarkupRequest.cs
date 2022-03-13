using System.ComponentModel.DataAnnotations;

namespace HappyTravel.Edo.Api.Models.Markups.Global;

public readonly struct SetGlobalMarkupRequest
{
    [Range(0.1d, 100d)]
    public decimal Percent { get; init; }
}