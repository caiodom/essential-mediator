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
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddScoped<IMediator, Mediator>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var notification = new UnregisteredNotification { Message = "Test" };

        // Act & Assert - Should not throw
        await mediator.Publish(notification);
        // Test passes if no exception is thrown
    }

    [Fact]
    public async Task Publish_WithHandlerThatThrows_ShouldLogErrorButNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddScoped<IMediator, Mediator>();
        services.AddScoped<INotificationHandler<ErrorNotification>, ErrorNotificationHandler>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var notification = new ErrorNotification { Message = "Test", ShouldThrow = true };

        // Act & Assert - Should log error but not throw due to try-catch in mediator
        await mediator.Publish(notification);
        // Test passes if no exception is thrown - errors are logged but not propagated
    }

    [Fact]
    public async Task Publish_WithMixedHandlers_OneThrows_ShouldLogErrorButComplete()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddScoped<IMediator, Mediator>();
        services.AddScoped<INotificationHandler<ErrorNotification>, ErrorNotificationHandler>();
        services.AddScoped<INotificationHandler<ErrorNotification>, AnotherErrorNotificationHandler>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var notification = new ErrorNotification { Message = "Test", ShouldThrow = true };

        // Act & Assert - Should log error but not throw due to try-catch in mediator
        await mediator.Publish(notification);
        // Test passes if no exception is thrown - errors are logged but not propagated
    }

    [Fact]
    public async Task Publish_WithHandlerThatDoesNotThrow_ShouldComplete()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddScoped<IMediator, Mediator>();
        services.AddScoped<INotificationHandler<ErrorNotification>, ErrorNotificationHandler>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var notification = new ErrorNotification { Message = "Test", ShouldThrow = false };

        // Act & Assert - Should not throw
        await mediator.Publish(notification);
        // Test passes if no exception is thrown
    }

    [Fact]
    public async Task Publish_WithCancellationToken_ShouldLogErrorButNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddScoped<IMediator, Mediator>();
        services.AddScoped<INotificationHandler<CancellationTestNotification>, CancellationTestNotificationHandler>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var notification = new CancellationTestNotification { Message = "Test" };

        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        // Act & Assert - Should log error but not throw due to try-catch in mediator
        await mediator.Publish(notification, cts.Token);
        // Test passes if no exception is thrown - cancellation errors are logged but not propagated
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

public class AnotherErrorNotificationHandler : INotificationHandler<ErrorNotification>
{
    public Task Handle(ErrorNotification notification, CancellationToken cancellationToken = default)
    {
        // This handler doesn't throw, but the other one does
        return Task.CompletedTask;
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
