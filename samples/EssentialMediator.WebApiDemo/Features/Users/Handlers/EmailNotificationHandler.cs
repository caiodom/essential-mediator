using EssentialMediator.Abstractions.Handlers;
using EssentialMediator.WebApiDemo.Features.Users.Notifications;

namespace EssentialMediator.WebApiDemo.Features.Users.Handlers;

public class EmailNotificationHandler : 
    INotificationHandler<UserCreatedNotification>,
    INotificationHandler<UserUpdatedNotification>,
    INotificationHandler<UserDeletedNotification>
{
    private readonly ILogger<EmailNotificationHandler> _logger;

    public EmailNotificationHandler(ILogger<EmailNotificationHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(UserCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sending welcome email to {UserEmail} for user {UserName} (ID: {UserId})", 
            notification.UserEmail, notification.UserName, notification.UserId);

        // Simulate email sending
        await Task.Delay(100, cancellationToken);

        _logger.LogInformation("Welcome email sent successfully to {UserEmail}", notification.UserEmail);
    }

    public async Task Handle(UserUpdatedNotification notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sending profile update notification to {UserEmail} for user {UserName} (ID: {UserId})", 
            notification.UserEmail, notification.UserName, notification.UserId);

        // Simulate email sending
        await Task.Delay(80, cancellationToken);

        _logger.LogInformation("Profile update email sent successfully to {UserEmail}", notification.UserEmail);
    }

    public async Task Handle(UserDeletedNotification notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sending goodbye email to {UserEmail} for user {UserName} (ID: {UserId})", 
            notification.UserEmail, notification.UserName, notification.UserId);

        // Simulate email sending
        await Task.Delay(90, cancellationToken);

        _logger.LogInformation("Goodbye email sent successfully to {UserEmail}", notification.UserEmail);
    }
}
