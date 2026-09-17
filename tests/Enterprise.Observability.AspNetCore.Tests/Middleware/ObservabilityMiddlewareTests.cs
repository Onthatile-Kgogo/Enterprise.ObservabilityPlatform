using Enterprise.Observability.AspNetCore.Middleware;
using Enterprise.Observability.Core.Abstractions;
using Enterprise.Observability.Core.Models;
using Microsoft.AspNetCore.Http;

namespace Enterprise.Observability.AspNetCore.Tests;

public class ObservabilityMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_ShouldRecordHttpRequest()
    {
        // Arrange
        var tracer = new TestObservabilityTracer();

        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/health";
        context.Response.StatusCode = StatusCodes.Status200OK;

        var middleware = new ObservabilityMiddleware(_ => Task.CompletedTask, tracer);

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

        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/failure";

        var middleware = new ObservabilityMiddleware(
            _ => throw new InvalidOperationException("Test exception"),
            tracer);

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(() => middleware.InvokeAsync(context));

        // Assert
        Assert.Single(tracer.Traces);

        var trace = tracer.Traces[0];

        Assert.Equal("HTTP GET /failure", trace.Name);
        Assert.Equal("GET", trace.Attributes["http.method"]);
        Assert.Equal("/failure", trace.Attributes["http.path"]);
    }

    private sealed class TestObservabilityTracer : IObservabilityTracer
    {
        public List<TraceEntry> Traces { get; } = [];
        public void Record(TraceEntry trace)
        {
            Traces.Add(trace);
        }
    }
}