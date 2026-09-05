using EssentialMediator.Abstractions.Handlers;
using EssentialMediator.Abstractions.Messages;
using EssentialMediator.Mediation;
using EssentialMediator.Tests.Models.Notifications;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EssentialMediator.Tests;

public class NotificationEdgeCaseTests
{
    [Fact]
    public async Task Publish_WithNoHandlers_ShouldLogWarningAndComplete()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddScoped<IMediator, Mediator>();

        using var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var notification = new UnregisteredNotification { Message = "Test" };

        await mediator.Publish(notification);
    }

    [Fact]
    public async Task Publish_WithSynchronouslyThrowingHandler_ShouldPropagateOriginalException()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddScoped<IMediator, Mediator>();
        services.AddScoped<INotificationHandler<ErrorNotification>, ErrorNotificationHandler>();

        using var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var notification = new ErrorNotification { Message = "Test", ShouldThrow = true };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => mediator.Publish(notification));

        Assert.Equal("Handler error", exception.Message);
    }

    [Fact]
    public async Task Publish_WithAsynchronouslyThrowingHandler_ShouldPropagateOriginalException()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddScoped<IMediator, Mediator>();
        services.AddScoped<INotificationHandler<AsyncErrorNotification>, AsyncErrorNotificationHandler>();

        using var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            mediator.Publish(new AsyncErrorNotification()));

        Assert.Equal("Async handler error", exception.Message);
    }

    [Fact]
    public async Task Publish_WithMixedHandlers_WhenOneThrows_ShouldStillInvokeOtherHandlersAndPropagateFailure()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddScoped<IMediator, Mediator>();
        services.AddScoped<ErrorNotificationHandler>();
        services.AddScoped<SuccessfulTrackingNotificationHandler>();
        services.AddScoped<INotificationHandler<ErrorNotification>>(sp => sp.GetRequiredService<ErrorNotificationHandler>());
        services.AddScoped<INotificationHandler<ErrorNotification>>(sp => sp.GetRequiredService<SuccessfulTrackingNotificationHandler>());

        using var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var successfulHandler = serviceProvider.GetRequiredService<SuccessfulTrackingNotificationHandler>();
        var notification = new ErrorNotification { Message = "Test", ShouldThrow = true };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => mediator.Publish(notification));

        Assert.Equal("Handler error", exception.Message);
        Assert.Equal(1, successfulHandler.HandledCount);
    }

    [Fact]
    public async Task Publish_WithHandlerThatDoesNotThrow_ShouldComplete()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddScoped<IMediator, Mediator>();
        services.AddScoped<INotificationHandler<ErrorNotification>, ErrorNotificationHandler>();

        using var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var notification = new ErrorNotification { Message = "Test", ShouldThrow = false };

        await mediator.Publish(notification);
    }

    [Fact]
    public async Task Publish_WithCancellationToken_ShouldPropagateCancellation()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddScoped<IMediator, Mediator>();
        services.AddScoped<INotificationHandler<CancellationTestNotification>, CancellationTestNotificationHandler>();

        using var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var notification = new CancellationTestNotification { Message = "Test" };

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => mediator.Publish(notification, cts.Token));
    }
}

public class UnregisteredNotification : INotification
{
    public string Message { get; set; } = string.Empty;
}

public class ErrorNotification : INotification
{
    public string Message { get; set; } = string.Empty;
    public bool ShouldThrow { get; set; }
}

public class ErrorNotificationHandler : INotificationHandler<ErrorNotification>
{
    public Task Handle(ErrorNotification notification, CancellationToken cancellationToken = default)
    {
        if (notification.ShouldThrow)
        {
            throw new InvalidOperationException("Handler error");
        }

        return Task.CompletedTask;
    }
}

public class SuccessfulTrackingNotificationHandler : INotificationHandler<ErrorNotification>
{
    public int HandledCount { get; private set; }

    public Task Handle(ErrorNotification notification, CancellationToken cancellationToken = default)
    {
        HandledCount++;
        return Task.CompletedTask;
    }
}

public class AsyncErrorNotification : INotification;

public class AsyncErrorNotificationHandler : INotificationHandler<AsyncErrorNotification>
{
    public async Task Handle(AsyncErrorNotification notification, CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        throw new InvalidOperationException("Async handler error");
    }
}

public class CancellationTestNotification : INotification
{
    public string Message { get; set; } = string.Empty;
}

public class CancellationTestNotificationHandler : INotificationHandler<CancellationTestNotification>
{
    public Task Handle(CancellationTestNotification notification, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }
}
