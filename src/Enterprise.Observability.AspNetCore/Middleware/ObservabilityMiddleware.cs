using System.Diagnostics;
using Enterprise.Observability.Core.Abstractions;
using Enterprise.Observability.Core.Enums;
using Enterprise.Observability.Core.Models;
using Microsoft.AspNetCore.Http;

namespace Enterprise.Observability.AspNetCore.Middleware;

public sealed class ObservabilityMiddleware
{
    private const string CorrelationHeader = "X-Correlation-ID";
    private const string CorrelationBaggageKey = "correlation.id";

    private const string RequestCountMetric = "http.server.requests";
    private const string RequestDurationMetric = "http.server.request.duration";
    private const string ActiveRequestsMetric = "http.server.active_requests";
    private const string ErrorMetric = "http.server.errors";

    private static long _activeRequests;

    private readonly RequestDelegate _next;
    private readonly IObservabilityTracer _tracer;
    private readonly IObservabilityMetric _metric;

    public ObservabilityMiddleware(RequestDelegate next, IObservabilityTracer tracer, IObservabilityMetric metric)
    {
        _next = next;
        _tracer = tracer;
        _metric = metric;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var stopwatch = Stopwatch.StartNew();
        var activity = Activity.Current;
        var activityCreated = false;

        if (activity is null)
        {
            activity = new Activity($"HTTP {context.Request.Method} {context.Request.Path}");
            activity.Start();
            activityCreated = true;
        }

        var correlationId = EstablishCorrelationId(context, activity);
        var observabilityContext = CreateObservabilityContext(activity, correlationId);

        context.Response.Headers[CorrelationHeader] = correlationId;

        var activeRequests = Interlocked.Increment(ref _activeRequests);

        RecordActiveRequests(context, activeRequests, observabilityContext);

        try
        {
            await _next(context);
        }
        catch
        {
            RecordError(context, observabilityContext);
            throw;
        }
        finally
        {
            stopwatch.Stop();

            RecordRequest(context, observabilityContext);
            RecordRequestDuration(context, stopwatch, observabilityContext);

            activeRequests = Interlocked.Decrement(ref _activeRequests);

            RecordActiveRequests(context, activeRequests, observabilityContext);
            RecordTrace(context, activity, stopwatch, observabilityContext);

            if (activityCreated)
            {
                activity.Stop();
            }
        }
    }

    #region Correlation

    private static string EstablishCorrelationId(HttpContext context, Activity activity)
    {
        var correlationId = activity.GetBaggageItem(CorrelationBaggageKey);

        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = context.Request.Headers[CorrelationHeader].FirstOrDefault();
        }

        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
        }

        activity.AddBaggage(CorrelationBaggageKey, correlationId);

        return correlationId;
    }

    private static ObservabilityContext CreateObservabilityContext(Activity activity, string correlationId)
    {
        return new ObservabilityContext
        {
            CorrelationId = correlationId,
            TraceId = activity.TraceId.ToString(),
            SpanId = activity.SpanId.ToString()
        };
    }

    #endregion

    #region Private Recording Methods

    private void RecordRequest(HttpContext context, ObservabilityContext observabilityContext)
    {
        _metric.Record(new MetricEntry
        {
            Timestamp = DateTimeOffset.UtcNow,
            Name = RequestCountMetric,
            Type = ObservabilityMetricType.Counter,
            Value = 1,
            Context = observabilityContext,
            Tags = CreateHttpTags(context)
        });
    }

    private void RecordRequestDuration(HttpContext context, Stopwatch stopwatch, ObservabilityContext observabilityContext)
    {
        _metric.Record(new MetricEntry
        {
            Timestamp = DateTimeOffset.UtcNow,
            Name = RequestDurationMetric,
            Type = ObservabilityMetricType.Histogram,
            Value = stopwatch.Elapsed.TotalMilliseconds,
            Context = observabilityContext,
            Tags = CreateHttpTags(context)
        });
    }

    private void RecordActiveRequests(HttpContext context, long activeRequests, ObservabilityContext observabilityContext)
    {
        _metric.Record(new MetricEntry
        {
            Timestamp = DateTimeOffset.UtcNow,
            Name = ActiveRequestsMetric,
            Type = ObservabilityMetricType.Gauge,
            Value = activeRequests,
            Context = observabilityContext,
            Tags = new Dictionary<string, string>
            {
                ["http.method"] = context.Request.Method
            }
        });
    }

    private void RecordError(HttpContext context, ObservabilityContext observabilityContext)
    {
        _metric.Record(new MetricEntry
        {
            Timestamp = DateTimeOffset.UtcNow,
            Name = ErrorMetric,
            Type = ObservabilityMetricType.Counter,
            Value = 1,
            Context = observabilityContext,
            Tags = new Dictionary<string, string>
            {
                ["http.method"] = context.Request.Method
            }
        });
    }

    private void RecordTrace(HttpContext context, Activity activity, Stopwatch stopwatch, ObservabilityContext observabilityContext)
    {
        var trace = new TraceEntry
        {
            Timestamp = DateTimeOffset.UtcNow,
            Name = $"HTTP {context.Request.Method} {context.Request.Path}",
            TraceId = activity.TraceId.ToString(),
            SpanId = activity.SpanId.ToString(),
            ParentSpanId = activity.ParentSpanId.ToString(),
            Context = observabilityContext,
            Attributes = new Dictionary<string, object?>
            {
                ["http.method"] = context.Request.Method,
                ["http.path"] = context.Request.Path.ToString(),
                ["http.status_code"] = context.Response.StatusCode,
                ["http.duration_ms"] = stopwatch.Elapsed.TotalMilliseconds
            }
        };

        _tracer.Record(trace);
    }

    private static Dictionary<string, string> CreateHttpTags(HttpContext context)
    {
        return new Dictionary<string, string>
        {
            ["http.method"] = context.Request.Method,
            ["http.status_code"] = context.Response.StatusCode.ToString()
        };
    }

    #endregion
}