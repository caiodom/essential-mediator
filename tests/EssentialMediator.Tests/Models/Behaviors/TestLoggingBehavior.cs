using EssentialMediator.Abstractions.Delegates;
using EssentialMediator.Abstractions.Messages;
using EssentialMediator.Abstractions.Pipelines;

namespace EssentialMediator.Tests.Models.Behaviors;

public class TestLoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public static List<string> Logs { get; } = new();

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        
        Logs.Add($"Before handling {requestName}");
        
        var response = await next();
        
        Logs.Add($"After handling {requestName}");
        
        return response;
    }
}
