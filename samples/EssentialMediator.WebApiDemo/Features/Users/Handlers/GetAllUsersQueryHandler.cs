using EssentialMediator.Abstractions.Handlers;
using EssentialMediator.WebApiDemo.Data;
using EssentialMediator.WebApiDemo.Features.Users.Queries;
using EssentialMediator.WebApiDemo.Models;
using Microsoft.EntityFrameworkCore;

namespace EssentialMediator.WebApiDemo.Features.Users.Handlers;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, List<UserDto>>
{
    private readonly DemoDbContext _context;
    private readonly ILogger<GetAllUsersQueryHandler> _logger;

    public GetAllUsersQueryHandler(DemoDbContext context, ILogger<GetAllUsersQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving all users with filters - IsActive: {IsActive}, SearchTerm: {SearchTerm}", 
            request.IsActive, request.SearchTerm);

        var query = _context.Users.AsQueryable();

        if (request.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == request.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(u => u.Name.Contains(request.SearchTerm) || u.Email.Contains(request.SearchTerm));
        }

        // Simulate some processing time to test performance behavior
        await Task.Delay(200, cancellationToken);

        var users = await query
            .OrderBy(u => u.Name)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt,
                IsActive = u.IsActive
            })
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} users", users.Count);

        return users;
    }
}
