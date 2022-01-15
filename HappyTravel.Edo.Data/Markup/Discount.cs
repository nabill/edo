using System;

namespace HappyTravel.Edo.Data.Markup
{
    public class Discount
    {
        public int Id { get; set; }
        public int TargetAgencyId { get; set; }
        public int TargetPolicyId { get; set; }
        public decimal DiscountPercent { get; set; }
        public bool IsActive { get; set; }
        public string Description { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset Modified { get; set; }
    }
}