using System.Diagnostics;
using Enterprise.Observability.Core.Abstractions;
using Enterprise.Observability.Core.Models;

namespace Enterprise.Observability.AspNetCore.Context;

public sealed class ObservabilityContextAccessor : IObservabilityContext
{
    public ObservabilityContext Current
    {
        get
        {
            var activity = Activity.Current;

            return new ObservabilityContext
            {
                CorrelationId = activity?.GetBaggageItem("correlation.id"),
                TraceId = activity?.TraceId.ToString(),
                SpanId = activity?.SpanId.ToString(),
                ServiceName = activity?.GetTagItem("service.name")?.ToString(),
                Environment = activity?.GetTagItem("deployment.environment.name")?.ToString()
            };
        }
    }
}