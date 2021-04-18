namespace HappyTravel.Edo.Data.Markup
{
    public class Discount
    {
        public int Id { get; set; }
        public int AgencyId { get; set; }
        public decimal DiscountPercent { get; set; }
        public bool IsEnabled { get; set; }
    }
}