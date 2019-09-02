using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.Models.Payments
{
    public class CardInfo
    {
        public int Id { get; set; }
        public string CardNumber { get; set; }
        public string ExpiryDate { get; set; }
        public string CardHolderName { get; set; }
        public CardOwner Owner { get; set; }
    }
}
