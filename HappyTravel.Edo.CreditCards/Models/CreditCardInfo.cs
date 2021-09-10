using System;

namespace HappyTravel.Edo.CreditCards.Models
{
    public record CreditCardInfo(string Number, DateTime ExpiryDate, string HolderName, string SecurityCode);
}