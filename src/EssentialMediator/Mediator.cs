using EssentialMediator.Abstractions.Handlers;
using EssentialMediator.Abstractions.Mediation;
using EssentialMediator.Abstractions.Messages;
using EssentialMediator.Exceptions;
using EssentialMediator.Mediation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace EssentialMediator;

/// <summary>
/// Default optimized implementation of IMediator
/// </summary>
public sealed class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<Mediator> _logger;
    private static readonly ConcurrentDictionary<Type, MethodInfo> HandleMethodCache = new();

    public Mediator(IServiceProvider serviceProvider, ILogger<Mediator> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        return SendInternalAsync(request, cancellationToken);
    }

    public Task<Unit> Send(IRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        return SendInternalVoidAsync(request, cancellationToken);
    }

    public async Task Publish(INotification notification, CancellationToken cancellationToken = default)
    {
        if (notification == null) throw new ArgumentNullException(nameof(notification));

        var notificationType = notification.GetType();
        var handlerInterface = typeof(INotificationHandler<>).MakeGenericType(notificationType);

        _logger.LogDebug("Publishing notification {NotificationType}", notificationType.Name);

        var handlers = _serviceProvider.GetServices(handlerInterface).ToList();
        if (!handlers.Any())
        {
            _logger.LogWarning("No handlers registered for notification {NotificationType}", notificationType.Name);
            return;
        }

        _logger.LogDebug("Found {HandlerCount} handlers for notification {NotificationType}",
            handlers.Count, notificationType.Name);

        var tasks = new List<Task>();
        foreach (var handler in handlers)
        {
            try
            {
                var method = HandleMethodCache.GetOrAdd(
                    handler.GetType(),
                    t => t.GetMethod("Handle", new[] { notificationType, typeof(CancellationToken) })
                          ?? throw new HandlerConfigurationException(handlerInterface, "Handle method not found"));

                if (method.Invoke(handler, new object[] { notification, cancellationToken }) is Task task)
                    tasks.Add(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task for handler {HandlerType}", handler.GetType().Name);
            }
        }

        try
        {
            await Task.WhenAll(tasks).ConfigureAwait(false);
            _logger.LogDebug("Successfully published notification {NotificationType}", notificationType.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing notification {NotificationType}", notificationType.Name);
            throw;
        }
    }

    private async Task<TResponse> SendInternalAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
    {
        var requestType = request.GetType();
        var responseType = typeof(TResponse);
        var handlerInterface = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);

        _logger.LogDebug("Sending request {RequestType} expecting {ResponseType}", requestType.Name, responseType.Name);

        var handlers = _serviceProvider.GetServices(handlerInterface).ToList();
        if (handlers.Count == 0)
        {
            _logger.LogError("No handler registered for request {RequestType}", requestType.Name);
            throw new HandlerNotFoundException(requestType);
        }
        if (handlers.Count > 1)
        {
            _logger.LogError("Multiple handlers ({HandlerCount}) found for request {RequestType}", handlers.Count, requestType.Name);
            throw new MultipleHandlersException(requestType, handlers.Count);
        }

        var handler = handlers.Single();
        try
        {
            var method = HandleMethodCache.GetOrAdd(
                handler.GetType(),
                t => t.GetMethod("Handle", new[] { requestType, typeof(CancellationToken) })
                      ?? throw new HandlerConfigurationException(handlerInterface, "Handle method not found"));


            var resultTask = method.Invoke(handler, new object[] { request, cancellationToken }) as Task<TResponse>
                                ?? throw new InvalidOperationException("Handler method returned null for a non-nullable Task<Unit>.");
            var result = await resultTask.ConfigureAwait(false);

            _logger.LogDebug("Successfully handled request {RequestType}", requestType.Name);
            return result;
        }
        catch (TargetInvocationException tie) when (tie.InnerException != null)
        {
            _logger.LogError(tie.InnerException, "Error handling request {RequestType}", requestType.Name);
            ExceptionDispatchInfo.Capture(tie.InnerException).Throw();
            throw; // unreachable
        }
        catch (Exception ex) when (!(ex is MediatorException))
        {
            _logger.LogError(ex, "Error sending request {RequestType}", requestType.Name);
            throw;
        }
    }

    private async Task<Unit> SendInternalVoidAsync(IRequest request, CancellationToken cancellationToken)
    {
        var requestType = request.GetType();
        var handlerInterface = typeof(IRequestHandler<>).MakeGenericType(requestType);

        _logger.LogDebug("Sending void request {RequestType}", requestType.Name);

        var handlers = _serviceProvider.GetServices(handlerInterface).ToList();
        if (handlers.Count == 0)
        {
            _logger.LogError("No handler registered for request {RequestType}", requestType.Name);
            throw new HandlerNotFoundException(requestType);
        }
        if (handlers.Count > 1)
        {
            _logger.LogError("Multiple handlers ({HandlerCount}) found for request {RequestType}", handlers.Count, requestType.Name);
            throw new MultipleHandlersException(requestType, handlers.Count);
        }

        var handler = handlers.Single();
        try
        {
            var method = HandleMethodCache.GetOrAdd(
                handler.GetType(),
                t => t.GetMethod("Handle", new[] { requestType, typeof(CancellationToken) })
                      ?? throw new HandlerConfigurationException(handlerInterface, "Handle method not found"));

            var resultTask = method.Invoke(handler, new object[] { request, cancellationToken }) as Task<Unit>
                ?? throw new InvalidOperationException("Handler method returned null for a non-nullable Task<Unit>.");
            var result = await resultTask.ConfigureAwait(false);

            _logger.LogDebug("Successfully handled void request {RequestType}", requestType.Name);
            return result;
        }
        catch (TargetInvocationException tie) when (tie.InnerException != null)
        {
            _logger.LogError(tie.InnerException, "Error handling void request {RequestType}", requestType.Name);
            ExceptionDispatchInfo.Capture(tie.InnerException).Throw();
            throw;
        }
        catch (Exception ex) when (!(ex is MediatorException))
        {
            _logger.LogError(ex, "Error sending void request {RequestType}", requestType.Name);
            throw;
        }
    }
}
