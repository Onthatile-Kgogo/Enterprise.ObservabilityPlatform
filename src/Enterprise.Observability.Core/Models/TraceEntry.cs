namespace Enterprise.Observability.Core.Models
{
    public sealed record TraceEntry
    {
        public required DateTimeOffset Timestamp { get; init; }
        public required string Name { get; init; }
        public required string TraceId { get; init; }
        public required string SpanId { get; init; }
        public string? ParentSpanId { get; init; }
        public ObservabilityContext? Context { get; init; }
        public IReadOnlyDictionary<string, object?> Attributes { get; init; } = new Dictionary<string, object?>();
    }
}
