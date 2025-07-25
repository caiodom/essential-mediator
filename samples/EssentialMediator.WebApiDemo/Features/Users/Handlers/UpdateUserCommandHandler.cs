using EssentialMediator.Abstractions.Handlers;
using EssentialMediator.Mediation;
using EssentialMediator.WebApiDemo.Data;
using EssentialMediator.WebApiDemo.Features.Users.Commands;
using EssentialMediator.WebApiDemo.Features.Users.Notifications;
using EssentialMediator.WebApiDemo.Models;
using Microsoft.EntityFrameworkCore;

namespace EssentialMediator.WebApiDemo.Features.Users.Handlers;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserDto>
{
    private readonly DemoDbContext _context;
    private readonly ILogger<UpdateUserCommandHandler> _logger;
    private readonly IMediator _mediator;

    public UpdateUserCommandHandler(DemoDbContext context, ILogger<UpdateUserCommandHandler> logger, IMediator mediator)
    {
        _context = context;
        _logger = logger;
        _mediator = mediator;
    }

    public async Task<UserDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating user with ID: {UserId}", request.Id);

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (user == null)
        {
            throw new InvalidOperationException($"User with ID '{request.Id}' not found");
        }


        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.Id != request.Id, cancellationToken);

        if (existingUser != null)
        {
            throw new InvalidOperationException($"Another user with email '{request.Email}' already exists");
        }

        // Simulate some processing time to test performance behavior
        await Task.Delay(150, cancellationToken);

        user.Name = request.Name;
        user.Email = request.Email;
        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User updated successfully with ID: {UserId}", user.Id);


        await _mediator.Publish(new UserUpdatedNotification
        {
            UserId = user.Id,
            UserName = user.Name,
            UserEmail = user.Email,
            UpdatedAt = user.UpdatedAt.Value
        }, cancellationToken);

        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            IsActive = user.IsActive
        };
    }
}
