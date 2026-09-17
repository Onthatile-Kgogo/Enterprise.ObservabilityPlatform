using Enterprise.Observability.Core.Models;

namespace Enterprise.Observability.Core.Abstractions
{
    internal interface IObservabilityMetric
    {
        void Record(MetricEntry metric);
    }
}
