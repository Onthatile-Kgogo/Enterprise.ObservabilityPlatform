using System.Diagnostics;
using Enterprise.Observability.Core.Abstractions;
using Enterprise.Observability.Core.Models;

namespace Enterprise.Observability.Tracing.Tracing
{
    public sealed class ObservabilityTracer : IObservabilityTracer
    {
        private readonly ActivitySource _activitySource;

        public ObservabilityTracer()
        {
            _activitySource = new ActivitySource("Enterprise.Observability");
        }

        public void Record(TraceEntry trace)
        {
            ArgumentNullException.ThrowIfNull(trace);

            using var activity = _activitySource.StartActivity(trace.Name, ActivityKind.Internal);

            if (activity is null)
                return;


            activity.SetStartTime(trace.Timestamp.UtcDateTime);

            foreach (var attribute in trace.Attributes)
            {
                activity.SetTag(attribute.Key, attribute.Value);
            }
            if (trace.ParentSpanId is not null)
            {
                activity.SetTag("parent.span_id", trace.ParentSpanId);
            }

            activity.SetTag("trace.id", trace.TraceId);
            activity.SetTag("span.id", trace.SpanId);
        }
    }
}