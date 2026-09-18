using Enterprise.Observability.Core.Abstractions;
using Enterprise.Observability.Core.Enums;
using Enterprise.Observability.Core.Models;
using System.Collections.Concurrent;
using System.Diagnostics.Metrics;

namespace Enterprise.Observability.Metrics.Metrics
{
    public sealed class ObservabilityMetric : IObservabilityMetric
    {
        private readonly ConcurrentDictionary<string, Counter<double>> _counters = new();
        private readonly ConcurrentDictionary<string, Gauge<double>> _gauges = new();
        private readonly ConcurrentDictionary<string, Histogram<double>> _histograms = new();

        private readonly Meter _meter;

        public ObservabilityMetric()
        {
            _meter = new Meter("Enterprise.Observability");
        }

        public void Record(MetricEntry metric)
        {
            ArgumentNullException.ThrowIfNull(metric);

            var tags = metric.Tags
           .Select(tag => new KeyValuePair<string, object?>(tag.Key, tag.Value))
           .ToArray();

            switch (metric.Type)
            {
                case ObservabilityMetricType.Counter:
                    RecordCounter(metric, tags);
                    break;

                case ObservabilityMetricType.Gauge:
                    RecordGauge(metric, tags);
                    break;

                case ObservabilityMetricType.Histogram:
                    RecordHistogram(metric, tags);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(metric),
                        metric.Type,
                        "Unsupported metric type.");
            }
        }


        private void RecordCounter(MetricEntry metric, KeyValuePair<string, object?>[] tags)
        {
            var counter = _counters.GetOrAdd(
                metric.Name,
                name => _meter.CreateCounter<double>(name));
            counter.Add(metric.Value, tags);
        }

        private void RecordGauge(MetricEntry metric, KeyValuePair<string, object?>[] tags)
        {
            var gauge = _gauges.GetOrAdd(
                metric.Name,
                name => _meter.CreateGauge<double>(name));

            gauge.Record(metric.Value, tags);
        }

        private void RecordHistogram(MetricEntry metric, KeyValuePair<string, object?>[] tags)
        {
            var histogram = _histograms.GetOrAdd(
                metric.Name,
                name => _meter.CreateHistogram<double>(name));

            histogram.Record(metric.Value, tags);
        }
    }
}
