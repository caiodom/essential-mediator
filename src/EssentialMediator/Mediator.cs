using EssentialMediator.Abstractions.Mediation;
using EssentialMediator.Abstractions.Messages;
using EssentialMediator.Dispatching;
using EssentialMediator.Exceptions;
using EssentialMediator.Mediation;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace EssentialMediator;

/// <summary>
/// Default optimized implementation of IMediator.
/// </summary>
public sealed class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<Mediator> _logger;

    private static readonly ConcurrentDictionary<(Type RequestType, Type ResponseType), object>
        RequestHandlerWrappers = new();

    private static readonly ConcurrentDictionary<Type, NotificationHandlerWrapper>
        NotificationHandlerWrappers = new();

    public Mediator(IServiceProvider serviceProvider, ILogger<Mediator> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    ///<inheritdoc/>
    public Task<TResponse> Send<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return SendCore(request, cancellationToken);
    }

    ///<inheritdoc/>
    public Task<Unit> Send(IRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return SendCore<Unit>(request, cancellationToken);
    }

    ///<inheritdoc/>
    public Task Publish(INotification notification, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notification);
        cancellationToken.ThrowIfCancellationRequested();
        return PublishCore(notification, cancellationToken);
    }

    private async Task<TResponse> SendCore<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken)
    {
        var requestType = request.GetType();
        var responseType = typeof(TResponse);
        var cacheKey = (RequestType: requestType, ResponseType: responseType);

        var wrapper = (RequestHandlerWrapper<TResponse>)RequestHandlerWrappers.GetOrAdd(
            cacheKey,
            static key => CreateRequestHandlerWrapper(key.RequestType, key.ResponseType));

        _logger.LogDebug(
            "Sending request {RequestType} expecting {ResponseType}",
            requestType.Name,
            responseType.Name);

        try
        {
            var result = await wrapper.Handle(
                    request,
                    _serviceProvider,
                    _logger,
                    cancellationToken)
                .ConfigureAwait(false);

            _logger.LogDebug("Successfully handled request {RequestType}", requestType.Name);
            return result;
        }
        catch (Exception ex) when (ex is not MediatorException)
        {
            _logger.LogError(ex, "Error sending request {RequestType}", requestType.Name);
            throw;
        }
    }

    private async Task PublishCore(
        INotification notification,
        CancellationToken cancellationToken)
    {
        var notificationType = notification.GetType();
        var wrapper = NotificationHandlerWrappers.GetOrAdd(
            notificationType,
            static type => CreateNotificationHandlerWrapper(type));

        _logger.LogDebug("Publishing notification {NotificationType}", notificationType.Name);

        try
        {
            await wrapper.Handle(
                    notification,
                    _serviceProvider,
                    _logger,
                    cancellationToken)
                .ConfigureAwait(false);

            _logger.LogDebug(
                "Successfully published notification {NotificationType}",
                notificationType.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error publishing notification {NotificationType}",
                notificationType.Name);
            throw;
        }
    }

    private static object CreateRequestHandlerWrapper(Type requestType, Type responseType)
    {
        try
        {
            var isVoidRequest = responseType == typeof(Unit)
                && typeof(IRequest).IsAssignableFrom(requestType);

            var wrapperType = isVoidRequest
                ? typeof(VoidRequestHandlerWrapper<>).MakeGenericType(requestType)
                : typeof(RequestHandlerWrapper<,>).MakeGenericType(requestType, responseType);

            return Activator.CreateInstance(wrapperType, nonPublic: true)
                ?? throw new HandlerConfigurationException(
                    requestType,
                    "Unable to create typed request handler wrapper");
        }
        catch (MediatorException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new HandlerConfigurationException(
                requestType,
                "Unable to create typed request handler wrapper",
                ex);
        }
    }

    private static NotificationHandlerWrapper CreateNotificationHandlerWrapper(Type notificationType)
    {
        try
        {
            var wrapperType = typeof(NotificationHandlerWrapper<>).MakeGenericType(notificationType);
            return (NotificationHandlerWrapper)(Activator.CreateInstance(wrapperType, nonPublic: true)
                ?? throw new HandlerConfigurationException(
                    notificationType,
                    "Unable to create typed notification handler wrapper"));
        }
        catch (MediatorException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new HandlerConfigurationException(
                notificationType,
                "Unable to create typed notification handler wrapper",
                ex);
        }
    }
}
