using EssentialMediator.Abstractions.Handlers;
using EssentialMediator.Abstractions.Messages;
using Microsoft.Extensions.Logging;

namespace EssentialMediator.Dispatching;

internal abstract class NotificationHandlerWrapper
{
    internal abstract Task Handle(
        INotification notification,
        IServiceProvider serviceProvider,
        ILogger<Mediator> logger,
        CancellationToken cancellationToken);
}

internal sealed class NotificationHandlerWrapper<TNotification> : NotificationHandlerWrapper
    where TNotification : INotification
{
    internal override async Task Handle(
        INotification notification,
        IServiceProvider serviceProvider,
        ILogger<Mediator> logger,
        CancellationToken cancellationToken)
    {
        var typedNotification = (TNotification)notification;
        var handlers = ServiceResolver.GetServices<INotificationHandler<TNotification>>(serviceProvider).ToArray();

        if (handlers.Length == 0)
        {
            logger.LogWarning(
                "No handlers registered for notification {NotificationType}",
                typeof(TNotification).Name);
            return;
        }

        logger.LogDebug(
            "Found {HandlerCount} handlers for notification {NotificationType}",
            handlers.Length,
            typeof(TNotification).Name);

        var tasks = new Task[handlers.Length];

        for (var index = 0; index < handlers.Length; index++)
        {
            tasks[index] = InvokeHandler(
                handlers[index],
                typedNotification,
                cancellationToken);
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    private static Task InvokeHandler(
        INotificationHandler<TNotification> handler,
        TNotification notification,
        CancellationToken cancellationToken)
    {
        try
        {
            return handler.Handle(notification, cancellationToken)
                ?? Task.FromException(new InvalidOperationException("Notification handler returned null Task"));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled(cancellationToken);
        }
        catch (Exception ex)
        {
            return Task.FromException(ex);
        }
    }
}
