using EssentialMediator.Abstractions.Handlers;
using EssentialMediator.WebApiDemo.Features.Users.Notifications;

namespace EssentialMediator.WebApiDemo.Features.Users.Handlers;

public class UserCreatedEmailHandler : INotificationHandler<UserCreatedNotification>
{
    private readonly ILogger<UserCreatedEmailHandler> _logger;

    public UserCreatedEmailHandler(ILogger<UserCreatedEmailHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(UserCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sending welcome email to {UserEmail} for user {UserName} (ID: {UserId})", 
            notification.UserEmail, notification.UserName, notification.UserId);

        // Simulate email sending
        await Task.Delay(50, cancellationToken);
        
        _logger.LogInformation("Welcome email sent successfully to {UserEmail}", notification.UserEmail);
    }
}
