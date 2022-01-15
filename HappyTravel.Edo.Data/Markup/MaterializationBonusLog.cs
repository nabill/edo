using System;

namespace HappyTravel.Edo.Data.Markup
{
    public class MaterializationBonusLog
    {
        public int PolicyId { get; set; }
        public string ReferenceCode { get; set; }
        public int AgencyAccountId { get; set; }
        public decimal Amount { get; set; }
        public DateTimeOffset Created { get; set; }
    }
}