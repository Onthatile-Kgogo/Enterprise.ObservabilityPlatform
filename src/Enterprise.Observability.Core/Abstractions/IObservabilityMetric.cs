using Enterprise.Observability.Core.Models;

namespace Enterprise.Observability.Core.Abstractions
{
    public interface IObservabilityMetric
    {
        void Record(MetricEntry metric);
    }
}
