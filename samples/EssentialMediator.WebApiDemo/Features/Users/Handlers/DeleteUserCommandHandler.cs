using EssentialMediator.Abstractions.Handlers;
using EssentialMediator.Abstractions.Mediation;
using EssentialMediator.Mediation;
using EssentialMediator.WebApiDemo.Data;
using EssentialMediator.WebApiDemo.Features.Users.Commands;
using EssentialMediator.WebApiDemo.Features.Users.Notifications;
using Microsoft.EntityFrameworkCore;

namespace EssentialMediator.WebApiDemo.Features.Users.Handlers;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Unit>
{
    private readonly DemoDbContext _context;
    private readonly ILogger<DeleteUserCommandHandler> _logger;
    private readonly IMediator _mediator;

    public DeleteUserCommandHandler(DemoDbContext context, ILogger<DeleteUserCommandHandler> logger, IMediator mediator)
    {
        _context = context;
        _logger = logger;
        _mediator = mediator;
    }

    public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting user with ID: {UserId}", request.Id);

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (user == null)
        {
            throw new InvalidOperationException($"User with ID '{request.Id}' not found");
        }


        var deletedUserData = new
        {
            user.Id,
            user.Name,
            user.Email
        };


        await Task.Delay(80, cancellationToken);

        _context.Users.Remove(user);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User deleted successfully with ID: {UserId}", request.Id);

        await _mediator.Publish(new UserDeletedNotification
        {
            UserId = deletedUserData.Id,
            UserName = deletedUserData.Name,
            UserEmail = deletedUserData.Email,
            DeletedAt = DateTime.UtcNow
        }, cancellationToken);

        return Unit.Value;
    }
}
