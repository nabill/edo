namespace HappyTravel.Edo.Data.Payments
{
    public class CreditCard
    {
        public int Id { get; set; }
        public string MaskedNumber { get; set; }
        public string ExpirationDate { get; set; }
        public string Token { get; set; }
        public string HolderName { get; set; }
        public int? CompanyId { get; set; }
        public int? CustomerId { get; set; }
    }
}
