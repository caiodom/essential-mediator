using EssentialMediator.Abstractions.Handlers;
using EssentialMediator.WebApiDemo.Features.Users.Notifications;

namespace EssentialMediator.WebApiDemo.Features.Users.Handlers;

public class UserCreatedAuditHandler : INotificationHandler<UserCreatedNotification>
{
    private readonly ILogger<UserCreatedAuditHandler> _logger;

    public UserCreatedAuditHandler(ILogger<UserCreatedAuditHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(UserCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("AUDIT: User created - ID: {UserId}, Name: {UserName}, Email: {UserEmail}, At: {CreatedAt}", 
            notification.UserId, notification.UserName, notification.UserEmail, notification.CreatedAt);

        // Simulate audit log writing
        await Task.Delay(20, cancellationToken);
    }
}
