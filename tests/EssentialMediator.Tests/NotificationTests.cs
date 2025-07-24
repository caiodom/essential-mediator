using EssentialMediator.Abstractions.Handlers;
using EssentialMediator.Mediation;
using EssentialMediator.Tests.Models.Notifications;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EssentialMediator.Tests;
public class NotificationTests
{
    private IServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddScoped<IMediator, Mediator>();
        services.AddScoped<INotificationHandler<TestNotification>, TestNotificationHandler1>();
        services.AddScoped<INotificationHandler<TestNotification>, TestNotificationHandler2>();

        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task Publish_WithValidNotification_ShouldCallAllHandlers()
    {
        // Arrange
        TestNotificationHandler1.HandledMessages.Clear();
        TestNotificationHandler2.HandledMessages.Clear();

        var serviceProvider = CreateServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var notification = new TestNotification { Message = "Test Notification" };

        // Act
        await mediator.Publish(notification);

        // Assert
        Assert.Contains("Handler1: Test Notification", TestNotificationHandler1.HandledMessages);
        Assert.Contains("Handler2: Test Notification", TestNotificationHandler2.HandledMessages);
    }

    [Fact]
    public async Task Publish_WithNullNotification_ShouldThrowArgumentNullException()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => mediator.Publish(null!));
    }

}


