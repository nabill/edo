using System;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Money.Enums;

namespace HappyTravel.Edo.Data.Booking
{
    public class BookingMarkup
    {
        public int Id { get; set; }
        public string ReferenceCode { get; set; }
        public int PolicyId { get; set; }
        public MarkupPolicyScopeType ScopeType { get; set; }
        public int? ScopeId { get; set; }
        public decimal Amount { get; set; }
        public Currencies Currency { get; set; }
        public int AgentId { get; set; }
        public int AgencyId { get; set; }
        public int CounterpartyId { get; set; }
        public DateTime? PayedAt { get; set; }
    }
}