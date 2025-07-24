using EssentialMediator.Abstractions.Handlers;

namespace EssentialMediator.Tests.Models.Notifications;

public class TestNotificationHandler1 : INotificationHandler<TestNotification>
{
    public static List<string> HandledMessages { get; } = new();

    public Task Handle(TestNotification notification, CancellationToken cancellationToken = default)
    {
        HandledMessages.Add($"Handler1: {notification.Message}");
        return Task.CompletedTask;
    }
}
