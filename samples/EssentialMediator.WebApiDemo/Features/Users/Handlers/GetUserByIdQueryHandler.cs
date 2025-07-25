using EssentialMediator.Abstractions.Handlers;
using EssentialMediator.WebApiDemo.Data;
using EssentialMediator.WebApiDemo.Features.Users.Queries;
using EssentialMediator.WebApiDemo.Models;
using Microsoft.EntityFrameworkCore;

namespace EssentialMediator.WebApiDemo.Features.Users.Handlers;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    private readonly DemoDbContext _context;
    private readonly ILogger<GetUserByIdQueryHandler> _logger;

    public GetUserByIdQueryHandler(DemoDbContext context, ILogger<GetUserByIdQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving user by ID: {UserId}", request.Id);

        // Simulate some processing time to test performance behavior
        await Task.Delay(50, cancellationToken);

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("User with ID {UserId} not found", request.Id);
            return null;
        }

        _logger.LogInformation("User found: {UserName} ({UserEmail})", user.Name, user.Email);

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
