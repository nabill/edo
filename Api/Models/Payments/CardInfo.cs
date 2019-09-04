namespace HappyTravel.Edo.Api.Models.Payments
{
    public class CardInfo
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public string ExpiryDate { get; set; }
        public string HolderName { get; set; }
        public CardOwner Owner { get; set; }
    }
}
