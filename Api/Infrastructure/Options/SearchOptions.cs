namespace HappyTravel.Edo.Api.Infrastructure.Options;

public class SearchOptions
{
    public bool IsCachedSearchEnabled { get; set; }
    
    public int NonDirectResultsMaxDelaySeconds { get; set; }
}