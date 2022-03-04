using System;

namespace HappyTravel.Edo.CreditCards.Models
{
    public record CreditCardInfo(string Number, DateTimeOffset ExpiryDate, string HolderName, string SecurityCode);
}