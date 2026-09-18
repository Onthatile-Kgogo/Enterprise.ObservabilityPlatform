using Enterprise.Observability.Core.Abstractions;
using Enterprise.Observability.Core.Enums;
using Enterprise.Observability.Core.Models;
using Microsoft.Extensions.Logging;

namespace Enterprise.Observability.Logging
{
    public sealed class ObservabilityLogger : IObservabilityLogger
    {
        private readonly ILogger<ObservabilityLogger> logger;
        public ObservabilityLogger(ILogger<ObservabilityLogger> logger)
        {
            this.logger = logger;
        }

        public void Log(ObservabilityLogLevel level, LogEntry entry)
        {
            ArgumentNullException.ThrowIfNull(entry);
            ArgumentNullException.ThrowIfNull(entry);

            var logLevel = MapLogLevel(level);
            var properties = CreateLogProperties(entry);

            logger.Log(
                logLevel,
                new EventId(),
                properties,
                entry.Exception,
                static (state, _) => state["Message"]?.ToString() ?? string.Empty);
        }

        private static Dictionary<string, object?> CreateLogProperties(LogEntry entry)
        {
            var properties = new Dictionary<string, object?>
            {
                ["Timestamp"] = entry.Timestamp,
                ["Message"] = entry.Message,
                ["Source"] = entry.Source,
                ["CorrelationId"] = entry.Context?.CorrelationId,
                ["TraceId"] = entry.Context?.TraceId,
                ["SpanId"] = entry.Context?.SpanId
            };

            foreach (var property in entry.Properties)
            {
                properties[property.Key] = property.Value;
            }

            return properties;
        }
        private static LogLevel MapLogLevel(ObservabilityLogLevel level)
        {
            return level switch
            {
                ObservabilityLogLevel.Trace => LogLevel.Trace,
                ObservabilityLogLevel.Debug => LogLevel.Debug,
                ObservabilityLogLevel.Information => LogLevel.Information,
                ObservabilityLogLevel.Warning => LogLevel.Warning,
                ObservabilityLogLevel.Error => LogLevel.Error,
                ObservabilityLogLevel.Critical => LogLevel.Critical,
                _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
            };
        }
    }
}
