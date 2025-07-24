using EssentialMediator.Abstractions.Handlers;
using EssentialMediator.WebApiDemo.Features.Users.Notifications;

namespace EssentialMediator.WebApiDemo.Features.Users.Handlers;

public class UserDeletedAuditHandler : INotificationHandler<UserDeletedNotification>
{
    private readonly ILogger<UserDeletedAuditHandler> _logger;

    public UserDeletedAuditHandler(ILogger<UserDeletedAuditHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(UserDeletedNotification notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("AUDIT: User deleted - ID: {UserId}, Name: {UserName}, Email: {UserEmail}, At: {DeletedAt}", 
            notification.UserId, notification.UserName, notification.UserEmail, notification.DeletedAt);

        // Simulate audit log writing
        await Task.Delay(20, cancellationToken);
    }
}
