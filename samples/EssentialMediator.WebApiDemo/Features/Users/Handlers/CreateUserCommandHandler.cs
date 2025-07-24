using EssentialMediator.Abstractions.Handlers;
using EssentialMediator.Mediation;
using EssentialMediator.WebApiDemo.Data;
using EssentialMediator.WebApiDemo.Features.Users.Commands;
using EssentialMediator.WebApiDemo.Features.Users.Notifications;
using EssentialMediator.WebApiDemo.Models;
using Microsoft.EntityFrameworkCore;

namespace EssentialMediator.WebApiDemo.Features.Users.Handlers;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly DemoDbContext _context;
    private readonly ILogger<CreateUserCommandHandler> _logger;
    private readonly IMediator _mediator;

    public CreateUserCommandHandler(DemoDbContext context, ILogger<CreateUserCommandHandler> logger, IMediator mediator)
    {
        _context = context;
        _logger = logger;
        _mediator = mediator;
    }

    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new user with email: {Email}", request.Email);


        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (existingUser != null)
        {
            throw new InvalidOperationException($"User with email '{request.Email}' already exists");
        }

        await Task.Delay(100, cancellationToken);

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User created successfully with ID: {UserId}", user.Id);

        await _mediator.Publish(new UserCreatedNotification
        {
            UserId = user.Id,
            UserName = user.Name,
            UserEmail = user.Email,
            CreatedAt = user.CreatedAt
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
