using System.Diagnostics;
using Enterprise.Observability.Core.Models;
using Enterprise.Observability.Tracing.Tracing;

namespace Enterprise.Observability.Tracing.Tests.Services;

public sealed class ObservabilityTracerTests
{
    [Fact]
    public void Record_ShouldThrowArgumentNullException_WhenTraceIsNull()
    {
        var tracer = new ObservabilityTracer();
        Assert.Throws<ArgumentNullException>(() => tracer.Record(null!));
    }

    [Fact]
    public void Record_ShouldCreateActivity_WithTraceName()
    {
        Activity? recordedActivity = null;

        using var listener = CreateListener(activity =>
        {
            recordedActivity = activity;
        });

        var tracer = new ObservabilityTracer();
        var trace = CreateTrace();

        tracer.Record(trace);

        Assert.NotNull(recordedActivity);
        Assert.Equal(trace.Name, recordedActivity.OperationName);
    }

    [Fact]
    public void Record_ShouldSetTraceAttributes()
    {
        Activity? recordedActivity = null;

        using var listener = CreateListener(activity =>
        {
            recordedActivity = activity;
        });

        var tracer = new ObservabilityTracer();

        var trace = CreateTrace(
            attributes: new Dictionary<string, object?>
            {
                ["http.method"] = "GET",
                ["http.status_code"] = 200
            });

        tracer.Record(trace);

        Assert.NotNull(recordedActivity);

        Assert.Equal("GET", recordedActivity.GetTagItem("http.method"));
        Assert.Equal(200, recordedActivity.GetTagItem("http.status_code"));
    }

    [Fact]
    public void Record_ShouldSetTraceAndSpanIdentifiers()
    {
        Activity? recordedActivity = null;

        using var listener = CreateListener(activity =>
        {
            recordedActivity = activity;
        });

        var tracer = new ObservabilityTracer();
        var trace = CreateTrace();

        tracer.Record(trace);

        Assert.NotNull(recordedActivity);
        Assert.Equal(trace.TraceId, recordedActivity.GetTagItem("trace.id"));
        Assert.Equal(trace.SpanId, recordedActivity.GetTagItem("span.id"));
    }

    [Fact]
    public void Record_ShouldSetParentSpanId_WhenProvided()
    {
        Activity? recordedActivity = null;

        using var listener = CreateListener(activity =>
        {
            recordedActivity = activity;
        });

        var tracer = new ObservabilityTracer();

        var trace = CreateTrace(parentSpanId: "parent-span-123");
        tracer.Record(trace);

        Assert.NotNull(recordedActivity);
        Assert.Equal("parent-span-123", recordedActivity.GetTagItem("parent.span_id"));
    }

    [Fact]
    public void Record_ShouldNotSetParentSpanId_WhenNotProvided()
    {
        Activity? recordedActivity = null;

        using var listener = CreateListener(activity =>
        {
            recordedActivity = activity;
        });

        var tracer = new ObservabilityTracer();
        var trace = CreateTrace();

        tracer.Record(trace);

        Assert.NotNull(recordedActivity);
        Assert.Null(recordedActivity.GetTagItem("parent.span_id"));
    }

    [Fact]
    public void Record_ShouldUseTraceTimestamp_AsActivityStartTime()
    {
        Activity? recordedActivity = null;

        using var listener = CreateListener(activity =>
        {
            recordedActivity = activity;
        });

        var tracer = new ObservabilityTracer();

        var timestamp = new DateTimeOffset(
            2026,
            9,
            17,
            17,
            30,
            0,
            TimeSpan.Zero);

        var trace = CreateTrace(timestamp: timestamp);

        tracer.Record(trace);

        Assert.NotNull(recordedActivity);
        Assert.Equal(timestamp.UtcDateTime, recordedActivity.StartTimeUtc);
    }

    [Fact]
    public void Record_ShouldUseInternalActivityKind()
    {
        Activity? recordedActivity = null;

        using var listener = CreateListener(activity =>
        {
            recordedActivity = activity;
        });

        var tracer = new ObservabilityTracer();
        var trace = CreateTrace();

        tracer.Record(trace);

        Assert.NotNull(recordedActivity);
        Assert.Equal(ActivityKind.Internal, recordedActivity.Kind);
    }

    private static ActivityListener CreateListener(Action<Activity> onActivityStopped)
    {
        var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "Enterprise.Observability",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStopped = onActivityStopped
        };

        ActivitySource.AddActivityListener(listener);

        return listener;
    }

    private static TraceEntry CreateTrace(DateTimeOffset? timestamp = null, string? parentSpanId = null, IReadOnlyDictionary<string, object?>? attributes = null)
    {
        return new TraceEntry
        {
            Timestamp = timestamp ?? DateTimeOffset.UtcNow,
            Name = "TestOperation",
            TraceId = "trace-123",
            SpanId = "span-123",
            ParentSpanId = parentSpanId,
            Attributes = attributes
                ?? new Dictionary<string, object?>()
        };
    }
}