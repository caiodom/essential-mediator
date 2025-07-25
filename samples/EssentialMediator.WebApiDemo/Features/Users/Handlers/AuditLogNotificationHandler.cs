using EssentialMediator.Abstractions.Handlers;
using EssentialMediator.WebApiDemo.Features.Users.Notifications;

namespace EssentialMediator.WebApiDemo.Features.Users.Handlers;

public class AuditLogNotificationHandler : 
    INotificationHandler<UserCreatedNotification>,
    INotificationHandler<UserUpdatedNotification>,
    INotificationHandler<UserDeletedNotification>
{
    private readonly ILogger<AuditLogNotificationHandler> _logger;

    public AuditLogNotificationHandler(ILogger<AuditLogNotificationHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(UserCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("AUDIT: User created - ID: {UserId}, Name: {UserName}, Email: {UserEmail}, At: {CreatedAt}", 
            notification.UserId, notification.UserName, notification.UserEmail, notification.CreatedAt);

        await Task.Delay(20, cancellationToken);
    }

    public async Task Handle(UserUpdatedNotification notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("AUDIT: User updated - ID: {UserId}, Name: {UserName}, Email: {UserEmail}, At: {UpdatedAt}", 
            notification.UserId, notification.UserName, notification.UserEmail, notification.UpdatedAt);

        await Task.Delay(20, cancellationToken);
    }

    public async Task Handle(UserDeletedNotification notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("AUDIT: User deleted - ID: {UserId}, Name: {UserName}, Email: {UserEmail}, At: {DeletedAt}", 
            notification.UserId, notification.UserName, notification.UserEmail, notification.DeletedAt);

        // Simulate audit log writing
        await Task.Delay(20, cancellationToken);
    }
}
