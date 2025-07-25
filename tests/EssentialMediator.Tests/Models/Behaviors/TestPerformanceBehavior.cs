using EssentialMediator.Abstractions.Delegates;
using EssentialMediator.Abstractions.Messages;
using EssentialMediator.Abstractions.Pipelines;

namespace EssentialMediator.Tests.Models.Behaviors;

public class TestPerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public static List<string> PerformanceLogs { get; } = new();

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        var response = await next();
        
        stopwatch.Stop();
        PerformanceLogs.Add($"{typeof(TRequest).Name} took {stopwatch.ElapsedMilliseconds}ms");
        
        return response;
    }
}
