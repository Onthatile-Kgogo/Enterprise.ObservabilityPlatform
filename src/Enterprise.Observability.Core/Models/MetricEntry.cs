namespace Enterprise.Observability.Core.Models
{
    public sealed record MetricEntry
    {
        public required DateTimeOffset Timestamp { get; init; }
        public required string Name { get; init; }
        public required double Value { get; init; }
        public ObservabilityContext? Context { get; init; }
        public IReadOnlyDictionary<string, string> Tags { get; init; } = new Dictionary<string, string>();
    }
}
