namespace Enterprise.Observability.Core.Models
{
    public sealed record ObservabilityContext
    {
        public string? CorrelationId { get; init; }
        public string? TraceId { get; init; }
        public string? SpanId { get; init; }
        public string? ServiceName { get; init; }
        public string? Environment { get; init; }
    }
}
