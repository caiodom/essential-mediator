using EssentialMediator.Abstractions.Delegates;
using EssentialMediator.Abstractions.Handlers;
using EssentialMediator.Abstractions.Mediation;
using EssentialMediator.Abstractions.Messages;
using EssentialMediator.Abstractions.Pipelines;
using EssentialMediator.Exceptions;
using Microsoft.Extensions.Logging;

namespace EssentialMediator.Dispatching;

internal abstract class RequestHandlerWrapper<TResponse>
{
    internal abstract Task<TResponse> Handle(
        IRequest<TResponse> request,
        IServiceProvider serviceProvider,
        ILogger<Mediator> logger,
        CancellationToken cancellationToken);
}

internal sealed class RequestHandlerWrapper<TRequest, TResponse> : RequestHandlerWrapper<TResponse>
    where TRequest : IRequest<TResponse>
{
    internal override Task<TResponse> Handle(
        IRequest<TResponse> request,
        IServiceProvider serviceProvider,
        ILogger<Mediator> logger,
        CancellationToken cancellationToken)
    {
        var typedRequest = (TRequest)request;
        var handlers = ServiceResolver.GetServices<IRequestHandler<TRequest, TResponse>>(serviceProvider).ToArray();

        if (handlers.Length == 0)
        {
            logger.LogError("No handler registered for request {RequestType}", typeof(TRequest).Name);
            throw new HandlerNotFoundException(typeof(TRequest));
        }

        if (handlers.Length > 1)
        {
            logger.LogError(
                "Multiple handlers ({HandlerCount}) found for request {RequestType}",
                handlers.Length,
                typeof(TRequest).Name);
            throw new MultipleHandlersException(typeof(TRequest), handlers.Length);
        }

        var behaviors = ServiceResolver.GetServices<IPipelineBehavior<TRequest, TResponse>>(serviceProvider).ToArray();

        if (behaviors.Length > 0)
        {
            logger.LogDebug(
                "Executing {BehaviorCount} pipeline behaviors for request {RequestType}",
                behaviors.Length,
                typeof(TRequest).Name);
        }

        RequestHandlerDelegate<TResponse> handler = () =>
            handlers[0].Handle(typedRequest, cancellationToken)
            ?? Task.FromException<TResponse>(
                new InvalidOperationException($"Handler returned null Task<{typeof(TResponse).Name}>")
            );

        for (var index = behaviors.Length - 1; index >= 0; index--)
        {
            var behavior = behaviors[index];
            var next = handler;
            handler = () =>
                behavior.Handle(typedRequest, next, cancellationToken)
                ?? Task.FromException<TResponse>(new InvalidOperationException("Behavior returned null"));
        }

        return handler();
    }
}

internal sealed class VoidRequestHandlerWrapper<TRequest> : RequestHandlerWrapper<Unit>
    where TRequest : IRequest
{
    internal override Task<Unit> Handle(
        IRequest<Unit> request,
        IServiceProvider serviceProvider,
        ILogger<Mediator> logger,
        CancellationToken cancellationToken)
    {
        var typedRequest = (TRequest)request;
        var handlers = ServiceResolver.GetServices<IRequestHandler<TRequest>>(serviceProvider).ToArray();

        if (handlers.Length == 0)
        {
            logger.LogError("No handler registered for request {RequestType}", typeof(TRequest).Name);
            throw new HandlerNotFoundException(typeof(TRequest));
        }

        if (handlers.Length > 1)
        {
            logger.LogError(
                "Multiple handlers ({HandlerCount}) found for request {RequestType}",
                handlers.Length,
                typeof(TRequest).Name);
            throw new MultipleHandlersException(typeof(TRequest), handlers.Length);
        }

        var behaviors = ServiceResolver.GetServices<IPipelineBehavior<TRequest, Unit>>(serviceProvider).ToArray();

        if (behaviors.Length > 0)
        {
            logger.LogDebug(
                "Executing {BehaviorCount} pipeline behaviors for void request {RequestType}",
                behaviors.Length,
                typeof(TRequest).Name);
        }

        RequestHandlerDelegate<Unit> handler = () =>
            handlers[0].Handle(typedRequest, cancellationToken)
            ?? Task.FromException<Unit>(new InvalidOperationException("Handler returned null Task<Unit>"));

        for (var index = behaviors.Length - 1; index >= 0; index--)
        {
            var behavior = behaviors[index];
            var next = handler;
            handler = () =>
                behavior.Handle(typedRequest, next, cancellationToken)
                ?? Task.FromException<Unit>(new InvalidOperationException("Behavior returned null"));
        }

        return handler();
    }
}
