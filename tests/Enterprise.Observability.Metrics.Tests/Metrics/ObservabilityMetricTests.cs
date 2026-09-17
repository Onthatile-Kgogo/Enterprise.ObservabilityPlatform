using Enterprise.Observability.Core.Enums;
using Enterprise.Observability.Core.Models;
using Enterprise.Observability.Metrics;
using Enterprise.Observability.Metrics.Metrics;
using System.Diagnostics.Metrics;

namespace Enterprise.Observability.Metrics.Tests;

public sealed class ObservabilityMetricTests
{
    [Fact]
    public void Record_Counter_RecordsExpectedValue()
    {
        // Arrange
        var metric = CreateMetric("orders.created", ObservabilityMetricType.Counter, 10);

        var recordedValue = 0d;

        using var listener = CreateListener(
            metric.Name,
            value => recordedValue = value);

        var sut = new ObservabilityMetric();

        // Act
        sut.Record(metric);

        // Assert
        Assert.Equal(10, recordedValue);
    }

    [Fact]
    public void Record_Gauge_RecordsExpectedValue()
    {
        // Arrange
        var metric = CreateMetric("orders.active", ObservabilityMetricType.Gauge, 25);

        var recordedValue = 0d;

        using var listener = CreateListener(
            metric.Name,
            value => recordedValue = value);

        var sut = new ObservabilityMetric();

        // Act
        sut.Record(metric);

        // Assert
        Assert.Equal(25, recordedValue);
    }

    [Fact]
    public void Record_Histogram_RecordsExpectedValue()
    {
        // Arrange
        var metric = CreateMetric("orders.duration", ObservabilityMetricType.Histogram, 145);

        var recordedValue = 0d;

        using var listener = CreateListener(metric.Name, value => recordedValue = value);

        var sut = new ObservabilityMetric();

        // Act
        sut.Record(metric);

        // Assert
        Assert.Equal(145, recordedValue);
    }

    [Fact]
    public void Record_WithTags_RecordsExpectedTags()
    {
        // Arrange
        var metric = new MetricEntry
        {
            Timestamp = DateTimeOffset.UtcNow,
            Name = "orders.created",
            Type = ObservabilityMetricType.Counter,
            Value = 10,
            Tags = new Dictionary<string, string>
            {
                ["Service"] = "OrderService",
                ["Environment"] = "Test"
            }
        };

        var recordedTags = new Dictionary<string, object?>();

        using var listener = CreateListener(
            metric.Name,
            (_, tags) =>
            {
                foreach (var tag in tags)
                {
                    recordedTags[tag.Key] = tag.Value;
                }
            });

        var sut = new ObservabilityMetric();

        // Act
        sut.Record(metric);

        // Assert
        Assert.Equal("OrderService", recordedTags["Service"]);
        Assert.Equal("Test", recordedTags["Environment"]);
    }

    [Fact]
    public void Record_WithNullMetric_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = new ObservabilityMetric();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => sut.Record(null!));
    }

    [Fact]
    public void Record_WithUnsupportedMetricType_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var metric = CreateMetric("orders.invalid", (ObservabilityMetricType)999, 10);

        var sut = new ObservabilityMetric();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => sut.Record(metric));
    }

    private static MetricEntry CreateMetric(string name, ObservabilityMetricType type, double value)
    {
        return new MetricEntry
        {
            Timestamp = DateTimeOffset.UtcNow,
            Name = name,
            Type = type,
            Value = value
        };
    }
    private static MeterListener CreateListener(string metricName, Action<double> onMeasurement)
    {
        return CreateListener(metricName, (value, _) => onMeasurement(value));
    }
    private static MeterListener CreateListener(string metricName, Action<double, ReadOnlySpan<KeyValuePair<string, object?>>> onMeasurement)
    {
        var listener = new MeterListener();

        listener.InstrumentPublished = (instrument, listener) =>
        {
            if (instrument.Meter.Name == "Enterprise.Observability" &&
                instrument.Name == metricName)
            {
                listener.EnableMeasurementEvents(instrument);
            }
        };

        listener.SetMeasurementEventCallback<double>(
            (instrument, value, tags, state) =>
            {
                onMeasurement(value, tags);
            });

        listener.Start();

        return listener;
    }
}