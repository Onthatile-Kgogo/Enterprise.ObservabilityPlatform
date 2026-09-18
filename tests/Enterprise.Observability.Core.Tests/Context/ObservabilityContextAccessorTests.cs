using Enterprise.Observability.AspNetCore.Context;
using System.Diagnostics;

namespace Enterprise.Observability.AspNetCore.Tests.Context;

public sealed class ObservabilityContextAccessorTests
{
    [Fact]
    public void Current_WhenActivityExists_ReturnsActivityContext()
    {
        using var activity = new Activity("test");
        activity.Start();

        activity.AddBaggage("correlation.id", "correlation-123");
        activity.SetTag("service.name", "test-service");
        activity.SetTag("deployment.environment.name", "test");

        var accessor = new ObservabilityContextAccessor();

        var result = accessor.Current;

        Assert.Equal(activity.TraceId.ToString(), result.TraceId);
        Assert.Equal(activity.SpanId.ToString(), result.SpanId);
        Assert.Equal("correlation-123", result.CorrelationId);
        Assert.Equal("test-service", result.ServiceName);
        Assert.Equal("test", result.Environment);
    }

    [Fact]
    public void Current_WhenNoActivityExists_ReturnsEmptyContext()
    {
        var accessor = new ObservabilityContextAccessor();

        var result = accessor.Current;

        Assert.Null(result.CorrelationId);
        Assert.Null(result.TraceId);
        Assert.Null(result.SpanId);
        Assert.Null(result.ServiceName);
        Assert.Null(result.Environment);
    }
}