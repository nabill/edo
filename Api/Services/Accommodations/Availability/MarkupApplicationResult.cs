using HappyTravel.Edo.Data.Markup;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public class MarkupApplicationResult<TDetails>
    {
        public TDetails Before { get; set; }
        public MarkupPolicy Policy { get; set; }
        public TDetails After { get; set; }
    }
}