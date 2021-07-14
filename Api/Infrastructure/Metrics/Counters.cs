using Prometheus;

namespace HappyTravel.Edo.Api.Infrastructure.Metrics
{
    public static class Counters
    {
        public static readonly Counter AccommodationAvailabilitySearchTimes = Prometheus.Metrics.CreateCounter(
            ApplicationPrefix + "accommodation_availability_search_total",
            "Counts start of an accommodation availability search",
            new CounterConfiguration
            {
                LabelNames = new[] {"method", "endpoint"},
            });


        public static readonly Counter WideAccommodationAvailabilitySearchTimes = Prometheus.Metrics.CreateCounter(
            ApplicationPrefix +"wide_accommodation_availability_search_total",
            "Counts start of a wide accommodation availability search",
            new CounterConfiguration
            {
                LabelNames = new[] {"method", "endpoint"},
            });
        
        
        public static readonly Histogram WideAccommodationAvailabilitySearchTaskDuration = Prometheus.Metrics.CreateHistogram(
            ApplicationPrefix + "wide_accommodation_availability_search_task_duration",
            "Wide accommodation availability search task duration",
            new HistogramConfiguration
            {
                LabelNames = new[] {"supplier"}
            });


        public static readonly Counter SupplierRequestCounter = Prometheus.Metrics.CreateCounter(
            ApplicationPrefix + "supplier_request_counter",
            "Supplier request counter",
            new CounterConfiguration
            {
                LabelNames = new [] {"step", "supplier", "errorDetails", "errorCode"}
            });
        
        
        public static readonly Histogram SupplierRequestHistogram = Prometheus.Metrics.CreateHistogram(
            ApplicationPrefix + "supplier_request_histogram",
            "Supplier request histogram",
            new HistogramConfiguration
            {
                LabelNames = new[] {"step", "supplier"}
            });


        public const string WideAvailabilitySearch = "wide-availability-search";
        public const string RoomSelection = "room-selection";
        public const string Evaluation = "evaluation";
        public const string Booking = "booking";
        public const string Cancellation = "cancellation";
        public const string GettingDeadline = "getting-deadline";
        public const string GettingInformation = "getting-information";
        public const string ProcessAsyncResponse = "process-async-response";
        
        
        private const string ApplicationPrefix = "edo_";
    }
}