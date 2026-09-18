using Enterprise.Observability.AspNetCore.Middleware;
using Enterprise.Observability.Core.Abstractions;
using Enterprise.Observability.Core.Enums;
using Enterprise.Observability.Core.Models;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;

namespace Enterprise.Observability.AspNetCore.Tests;

public class ObservabilityMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_ShouldRecordHttpRequest()
    {
        // Arrange
        var tracer = new TestObservabilityTracer();
        var metric = new TestObservabilityMetric();

        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/health";
        context.Response.StatusCode = StatusCodes.Status200OK;

        var middleware = new ObservabilityMiddleware(_ => Task.CompletedTask, tracer, metric);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Single(tracer.Traces);

        var trace = tracer.Traces[0];

        Assert.Equal("HTTP GET /health", trace.Name);
        Assert.Equal("GET", trace.Attributes["http.method"]);
        Assert.Equal("/health", trace.Attributes["http.path"]);
        Assert.Equal(200, trace.Attributes["http.status_code"]);
        Assert.True((double)trace.Attributes["http.duration_ms"]! >= 0);
        Assert.False(string.IsNullOrWhiteSpace(trace.TraceId));
        Assert.False(string.IsNullOrWhiteSpace(trace.SpanId));
    }

    [Fact]
    public async Task InvokeAsync_ShouldRecordRequest_WhenNextMiddlewareThrows()
    {
        // Arrange
        var tracer = new TestObservabilityTracer();
        var metric = new TestObservabilityMetric();

        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/failure";

        var middleware = new ObservabilityMiddleware(_ => throw new InvalidOperationException("Test exception"), tracer, metric);

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(() => middleware.InvokeAsync(context));

        // Assert
        Assert.Single(tracer.Traces);

        var trace = tracer.Traces[0];

        Assert.Equal("HTTP GET /failure", trace.Name);
        Assert.Equal("GET", trace.Attributes["http.method"]);
        Assert.Equal("/failure", trace.Attributes["http.path"]);
    }

    [Fact]
    public async Task InvokeAsync_ShouldRecordRequestMetric()
    {
        // Arrange
        var tracer = new TestObservabilityTracer();
        var metric = new TestObservabilityMetric();

        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/health";
        context.Response.StatusCode = StatusCodes.Status200OK;

        var middleware = new ObservabilityMiddleware(_ => Task.CompletedTask, tracer, metric);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var requestMetric = Assert.Single(metric.Metrics, x => x.Name == "http.server.requests");

        Assert.Equal(ObservabilityMetricType.Counter, requestMetric.Type);
        Assert.Equal(1, requestMetric.Value);
        Assert.Equal("GET", requestMetric.Tags["http.method"]);
        Assert.Equal("200", requestMetric.Tags["http.status_code"]);
    }

    [Fact]
    public async Task InvokeAsync_ShouldRecordRequestDurationMetric()
    {
        // Arrange
        var tracer = new TestObservabilityTracer();
        var metric = new TestObservabilityMetric();

        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/health";
        context.Response.StatusCode = StatusCodes.Status200OK;

        var middleware = new ObservabilityMiddleware(async _ => await Task.Delay(10), tracer, metric);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var durationMetric = Assert.Single(metric.Metrics, x => x.Name == "http.server.request.duration");

        Assert.Equal(ObservabilityMetricType.Histogram, durationMetric.Type);
        Assert.True(durationMetric.Value >= 0);
        Assert.Equal("GET", durationMetric.Tags["http.method"]);
        Assert.Equal("200", durationMetric.Tags["http.status_code"]);
    }

    [Fact]
    public async Task InvokeAsync_ShouldRecordActiveRequestGauge()
    {
        // Arrange
        var tracer = new TestObservabilityTracer();
        var metric = new TestObservabilityMetric();

        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/health";

        var middleware = new ObservabilityMiddleware(_ => Task.CompletedTask, tracer, metric);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var activeMetrics = metric.Metrics
            .Where(x => x.Name == "http.server.active_requests")
            .ToList();

        Assert.Equal(2, activeMetrics.Count);
        Assert.Equal(1, activeMetrics[0].Value);
        Assert.Equal(0, activeMetrics[1].Value);
        Assert.All(activeMetrics, metricEntry => Assert.Equal("GET", metricEntry.Tags["http.method"]));
    }

    [Fact]
    public async Task InvokeAsync_ShouldRecordErrorMetric_WhenNextMiddlewareThrows()
    {
        // Arrange
        var tracer = new TestObservabilityTracer();
        var metric = new TestObservabilityMetric();

        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/failure";

        var middleware = new ObservabilityMiddleware(_ => throw new InvalidOperationException("Test exception"), tracer, metric);

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(() => middleware.InvokeAsync(context));

        // Assert
        var errorMetric = Assert.Single(metric.Metrics, x => x.Name == "http.server.errors");

        Assert.Equal(ObservabilityMetricType.Counter, errorMetric.Type);

        Assert.Equal(1, errorMetric.Value);
        Assert.Equal("GET", errorMetric.Tags["http.method"]);
    }

    [Fact]
    public async Task InvokeAsync_ShouldGenerateCorrelationId_WhenRequestDoesNotContainOne()
    {
        // Arrange
        var tracer = new TestObservabilityTracer();
        var metric = new TestObservabilityMetric();

        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/health";

        var middleware = new ObservabilityMiddleware(
            _ => Task.CompletedTask,
            tracer,
            metric);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var correlationId = context.Response.Headers["X-Correlation-ID"].ToString();

        Assert.False(string.IsNullOrWhiteSpace(correlationId));

        var trace = Assert.Single(tracer.Traces);

        Assert.Equal(correlationId, trace.Context?.CorrelationId);

        var requestMetric = Assert.Single(
            metric.Metrics,
            x => x.Name == "http.server.requests");

        Assert.Equal(correlationId, requestMetric.Context?.CorrelationId);
    }

    [Fact]
    public async Task InvokeAsync_ShouldPreserveIncomingCorrelationId()
    {
        // Arrange
        var tracer = new TestObservabilityTracer();
        var metric = new TestObservabilityMetric();

        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/health";
        context.Request.Headers["X-Correlation-ID"] = "correlation-123";

        var middleware = new ObservabilityMiddleware(
            _ => Task.CompletedTask,
            tracer,
            metric);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(
            "correlation-123",
            context.Response.Headers["X-Correlation-ID"].ToString());

        var trace = Assert.Single(tracer.Traces);

        Assert.Equal(
            "correlation-123",
            trace.Context?.CorrelationId);

        Assert.All(
            metric.Metrics,
            metricEntry => Assert.Equal(
                "correlation-123",
                metricEntry.Context?.CorrelationId));
    }

    [Fact]
    public async Task InvokeAsync_ShouldPreserveExistingActivityCorrelationId()
    {
        // Arrange
        var tracer = new TestObservabilityTracer();
        var metric = new TestObservabilityMetric();

        using var activity = new Activity("existing-request");
        activity.Start();
        activity.AddBaggage("correlation.id", "activity-correlation-123");

        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/health";

        var middleware = new ObservabilityMiddleware(
            _ => Task.CompletedTask,
            tracer,
            metric);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(
            "activity-correlation-123",
            context.Response.Headers["X-Correlation-ID"].ToString());

        var trace = Assert.Single(tracer.Traces);

        Assert.Equal(
            "activity-correlation-123",
            trace.Context?.CorrelationId);
    }

    #region Test Helpers
    private sealed class TestObservabilityTracer : IObservabilityTracer
    {
        public List<TraceEntry> Traces { get; } = [];
        public void Record(TraceEntry trace)
        {
            Traces.Add(trace);
        }
    }
    private sealed class TestObservabilityMetric : IObservabilityMetric
    {
        public List<MetricEntry> Metrics { get; } = [];

        public void Record(MetricEntry metric)
        {
            Metrics.Add(metric);
        }
    }
    #endregion
}