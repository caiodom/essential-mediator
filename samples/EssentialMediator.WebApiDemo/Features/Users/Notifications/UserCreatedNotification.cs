using EssentialMediator.Abstractions.Messages;

namespace EssentialMediator.WebApiDemo.Features.Users.Notifications;

public class UserCreatedNotification : INotification
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
