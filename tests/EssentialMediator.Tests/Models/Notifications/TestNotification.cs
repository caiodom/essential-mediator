using EssentialMediator.Abstractions.Messages;

namespace EssentialMediator.Tests.Models.Notifications;

public class TestNotification : INotification
{
    public string Message { get; set; } = string.Empty;
}
