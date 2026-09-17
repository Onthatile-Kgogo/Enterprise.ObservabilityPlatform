using Enterprise.Observability.Core.Models;

namespace Enterprise.Observability.Core.Abstractions
{
    public interface IObservabilityTracer
    {
        void Record(TraceEntry trace);
    }
}
