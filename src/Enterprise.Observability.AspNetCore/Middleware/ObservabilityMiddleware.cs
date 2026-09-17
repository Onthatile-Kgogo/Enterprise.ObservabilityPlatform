using System.Diagnostics;
using Enterprise.Observability.Core.Abstractions;
using Enterprise.Observability.Core.Models;
using Microsoft.AspNetCore.Http;

namespace Enterprise.Observability.AspNetCore.Middleware;

public sealed class ObservabilityMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IObservabilityTracer _tracer;

    public ObservabilityMiddleware(RequestDelegate next, IObservabilityTracer tracer)
    {
        _next = next;
        _tracer = tracer;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var activity = Activity.Current;

        if (activity is null)
        {
            activity = new Activity(                $"HTTP {context.Request.Method} {context.Request.Path}");
            activity.Start();
        }

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            var trace = new TraceEntry
            {
                Timestamp = DateTimeOffset.UtcNow,
                Name = $"HTTP {context.Request.Method} {context.Request.Path}",
                TraceId = activity.TraceId.ToString(),
                SpanId = activity.SpanId.ToString(),
                ParentSpanId = activity.ParentSpanId.ToString(),
                Attributes = new Dictionary<string, object?>
                {
                    ["http.method"] = context.Request.Method,
                    ["http.path"] = context.Request.Path.ToString(),
                    ["http.status_code"] = context.Response.StatusCode,
                    ["http.duration_ms"] = stopwatch.Elapsed.TotalMilliseconds
                }
            };

            _tracer.Record(trace);

            if (activity.Duration == TimeSpan.Zero)
            {
                activity.Stop();
            }
        }
    }
}