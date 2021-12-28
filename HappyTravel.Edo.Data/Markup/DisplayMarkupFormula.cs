namespace HappyTravel.Edo.Data.Markup
{
    public class DisplayMarkupFormula : IEntity
    {
        public int Id { get; set; }
        public int? AgencyId { get; set; }
        public int? AgentId { get; set; }
        public string DisplayFormula { get; set; }
    }
}