using System.ComponentModel.DataAnnotations;

namespace HappyTravel.Edo.Api.Models.Markups.Agency;

public readonly struct SetAgencyMarkupRequest
{
    [Range(0.1d, 100d)]
    public decimal Percent { get; init; }
}