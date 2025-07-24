using EssentialMediator.Abstractions.Handlers;
using EssentialMediator.WebApiDemo.Features.Users.Notifications;

namespace EssentialMediator.WebApiDemo.Features.Users.Handlers;

public class UserUpdatedAuditHandler : INotificationHandler<UserUpdatedNotification>
{
    private readonly ILogger<UserUpdatedAuditHandler> _logger;

    public UserUpdatedAuditHandler(ILogger<UserUpdatedAuditHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(UserUpdatedNotification notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("AUDIT: User updated - ID: {UserId}, Name: {UserName}, Email: {UserEmail}, At: {UpdatedAt}", 
            notification.UserId, notification.UserName, notification.UserEmail, notification.UpdatedAt);

        // Simulate audit log writing
        await Task.Delay(20, cancellationToken);
    }
}
