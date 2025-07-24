using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;
using EssentialMediator.Exceptions;
using EssentialMediator.Abstractions.Handlers;
using EssentialMediator.Abstractions.Mediation;
using EssentialMediator.Abstractions.Messages;

namespace EssentialMediator;

/// <summary>
/// Default implementation of IMediator - Simple and efficient
/// </summary>
public sealed class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<Mediator> _logger;

    public Mediator(IServiceProvider serviceProvider, ILogger<Mediator> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestType = request.GetType();
        var responseType = typeof(TResponse);
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);

        _logger.LogDebug("Sending request {RequestType} expecting {ResponseType}", requestType.Name, responseType.Name);

        var handlers = _serviceProvider.GetServices(handlerType).ToList();
        if (!handlers.Any())
        {
            _logger.LogError("No handler registered for request type {RequestType}", requestType.Name);
            throw new HandlerNotFoundException(requestType);
        }

        if (handlers.Count > 1)
        {
            _logger.LogError("Multiple handlers ({HandlerCount}) found for request type {RequestType} that expects single handler",
                handlers.Count, requestType.Name);
            throw new MultipleHandlersException(requestType, handlers.Count);
        }

        var handler = handlers.First();

        try
        {
            var handleMethod = handler?.GetType().GetMethod("Handle");
            if (handleMethod is null)
                throw new HandlerConfigurationException(handlerType, "Handle method not found");

            var invokeResult = handleMethod.Invoke(handler, new object[] { request, cancellationToken });
            if (invokeResult is not Task<TResponse> task)
                throw new HandlerConfigurationException(handlerType, $"Handler method did not return expected Task<{responseType.Name}>");

            var result = await task.ConfigureAwait(false);

            _logger.LogDebug("Successfully handled request {RequestType}", requestType.Name);
            return result;
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            _logger.LogError(ex.InnerException, "Error handling request {RequestType}", requestType.Name);
            throw ex.InnerException;
        }
        catch (Exception ex) when (!(ex is MediatorException))
        {
            _logger.LogError(ex, "Error handling request {RequestType}", requestType.Name);
            throw;
        }
    }

    public async Task<Unit> Send(IRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestType = request.GetType();
        var handlerType = typeof(IRequestHandler<>).MakeGenericType(requestType);

        _logger.LogDebug("Sending void request {RequestType}", requestType.Name);

        var handlers = _serviceProvider.GetServices(handlerType).ToList();
        if (!handlers.Any())
        {
            _logger.LogError("No handler registered for request type {RequestType}", requestType.Name);
            throw new HandlerNotFoundException(requestType);
        }

        if (handlers.Count > 1)
        {
            _logger.LogError("Multiple handlers ({HandlerCount}) found for request type {RequestType} that expects single handler",
                handlers.Count, requestType.Name);
            throw new MultipleHandlersException(requestType, handlers.Count);
        }

        var handler = handlers.First();

        try
        {
            var handleMethod = handler?.GetType().GetMethod("Handle");
            if (handleMethod is null)
                throw new HandlerConfigurationException(handlerType, "Handle method not found");

            var invokeResult = handleMethod.Invoke(handler, new object[] { request, cancellationToken });
            if (invokeResult is not Task<Unit> task)
                throw new HandlerConfigurationException(handlerType, "Handler method did not return expected Task<Unit>");

            var result = await task.ConfigureAwait(false);

            _logger.LogDebug("Successfully handled void request {RequestType}", requestType.Name);
            return result;
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            _logger.LogError(ex.InnerException, "Error handling void request {RequestType}", requestType.Name);
            throw ex.InnerException;
        }
        catch (Exception ex) when (!(ex is MediatorException))
        {
            _logger.LogError(ex, "Error handling void request {RequestType}", requestType.Name);
            throw;
        }
    }

    public async Task Publish(INotification notification, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notification);

        var notificationType = notification.GetType();
        var handlerType = typeof(INotificationHandler<>).MakeGenericType(notificationType);

        _logger.LogDebug("Publishing notification {NotificationType}", notificationType.Name);

        var handlers = _serviceProvider.GetServices(handlerType).ToList();

        if (!handlers.Any())
        {
            _logger.LogWarning("No handlers registered for notification type {NotificationType}", notificationType.Name);
            return;
        }

        _logger.LogDebug("Found {HandlerCount} handlers for notification {NotificationType}",
            handlers.Count, notificationType.Name);

        var tasks = new List<Task>();

        foreach (var handler in handlers)
        {
            try
            {
                var handleMethod = handler?.GetType().GetMethod("Handle");
                if (handleMethod is null)
                {
                    _logger.LogWarning("Handle method not found on handler {HandlerType}", handler?.GetType().Name ?? "Unknown");
                    continue;
                }

                var invokeResult = handleMethod.Invoke(handler, new object[] { notification, cancellationToken });
                if (invokeResult is Task task)
                    tasks.Add(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task for notification handler {HandlerType}", handler?.GetType().Name ?? "Unknown");
            }
        }

        try
        {
            await Task.WhenAll(tasks).ConfigureAwait(false);
            _logger.LogDebug("Successfully published notification {NotificationType} to {HandlerCount} handlers",
                notificationType.Name, handlers.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing notification {NotificationType}", notificationType.Name);
            throw;
        }
    }
}
