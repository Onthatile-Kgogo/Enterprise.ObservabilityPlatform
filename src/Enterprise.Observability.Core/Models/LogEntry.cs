namespace Enterprise.Observability.Core.Models
{
    public sealed record LogEntry
    {
        public required DateTimeOffset Timestamp { get; init; }
        public required string Message { get; init; }
        public Exception? Exception { get; init; }
        public string? Source { get; init; }
        public ObservabilityContext? Context { get; init; }
        public IReadOnlyDictionary<string, object?> Properties { get; init; } = new Dictionary<string, object?>();
    }
}
