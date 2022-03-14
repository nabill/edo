using System.ComponentModel.DataAnnotations;

namespace HappyTravel.Edo.Api.Models.Markups.Agency;

public readonly struct SetAgencyMarkupRequest
{
    [Range(-100d, 100d)]
    public decimal Percent { get; init; }
}