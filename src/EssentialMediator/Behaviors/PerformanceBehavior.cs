using EssentialMediator.Abstractions.Delegates;
using EssentialMediator.Abstractions.Messages;
using EssentialMediator.Abstractions.Pipelines;
using Microsoft.Extensions.Logging;

namespace EssentialMediator.Behaviors;

/// <summary>
/// Performance monitoring behavior that logs slow requests
/// </summary>
/// <typeparam name="TRequest">Request type</typeparam>
/// <typeparam name="TResponse">Response type</typeparam>
public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
    private readonly int _slowRequestThresholdMs;

    public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger, int slowRequestThresholdMs = 500)
    {
        _logger = logger;
        _slowRequestThresholdMs = slowRequestThresholdMs;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        var response = await next();

        stopwatch.Stop();
        var elapsedMs = stopwatch.ElapsedMilliseconds;

        if (elapsedMs > _slowRequestThresholdMs)
        {
            _logger.LogWarning("Slow request detected: {RequestName} took {ElapsedMs}ms (threshold: {ThresholdMs}ms)",
                requestName, elapsedMs, _slowRequestThresholdMs);
        }
        else
        {
            _logger.LogDebug("Request {RequestName} completed in {ElapsedMs}ms", requestName, elapsedMs);
        }

        return response;
    }
}
