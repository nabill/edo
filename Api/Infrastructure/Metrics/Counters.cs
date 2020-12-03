using Prometheus;

namespace HappyTravel.Edo.Api.Infrastructure.Metrics
{
    public static class Counters
    {
        public static readonly Counter WideAvailabilitySearchTimes = Prometheus.Metrics.CreateCounter("availability_search_counter", "Counts starts of the wide availability search",
            new CounterConfiguration
            {
                LabelNames = new[] {"method", "endpoint"},
            });
    }
}