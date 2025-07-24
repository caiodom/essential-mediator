using EssentialMediator.Mediation;
using EssentialMediator.WebApiDemo.Features.Users.Commands;
using EssentialMediator.WebApiDemo.Features.Users.Queries;
using EssentialMediator.WebApiDemo.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace EssentialMediator.WebApiDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IMediator mediator, ILogger<UsersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Retrieve all users with optional filtering
    /// </summary>
    /// <param name="isActive">Filter by active status</param>
    /// <param name="searchTerm">Search term for name or email</param>
    /// <returns>List of users</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UserDto>>> GetAllUsers(
        [FromQuery] bool? isActive = null,
        [FromQuery] string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GET /api/users - isActive: {IsActive}, searchTerm: {SearchTerm}", 
            isActive, searchTerm);

        var query = new GetAllUsersQuery 
        { 
            IsActive = isActive, 
            SearchTerm = searchTerm 
        };

        var users = await _mediator.Send(query, cancellationToken);
        return Ok(users);
    }

    /// <summary>
    /// Retrieve a specific user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User details</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserDto>> GetUserById(
        [FromRoute] int id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GET /api/users/{UserId}", id);

        if (id <= 0)
        {
            return BadRequest("Invalid user ID");
        }

        var query = new GetUserByIdQuery { Id = id };
        var user = await _mediator.Send(query, cancellationToken);

        if (user == null)
        {
            return NotFound($"User with ID {id} not found");
        }

        return Ok(user);
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    /// <param name="createUserDto">User creation data</param>
    /// <returns>Created user details</returns>
    [HttpPost]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserDto>> CreateUser(
        [FromBody] CreateUserDto createUserDto,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("POST /api/users - Creating user with email: {Email}", createUserDto.Email);

        var command = new CreateUserCommand
        {
            Name = createUserDto.Name,
            Email = createUserDto.Email
        };

        try
        {
            var user = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing user
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="updateUserDto">User update data</param>
    /// <returns>Updated user details</returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserDto>> UpdateUser(
        [FromRoute] int id,
        [FromBody] UpdateUserDto updateUserDto,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("PUT /api/users/{UserId} - Updating user", id);

        if (id <= 0)
        {
            return BadRequest("Invalid user ID");
        }

        var command = new UpdateUserCommand
        {
            Id = id,
            Name = updateUserDto.Name,
            Email = updateUserDto.Email,
            IsActive = updateUserDto.IsActive
        };

        try
        {
            var user = await _mediator.Send(command, cancellationToken);
            return Ok(user);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete a user
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteUser(
        [FromRoute] int id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("DELETE /api/users/{UserId}", id);

        if (id <= 0)
        {
            return BadRequest("Invalid user ID");
        }

        var command = new DeleteUserCommand { Id = id };

        try
        {
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Health check endpoint to test if mediator is working
    /// </summary>
    /// <returns>System health information</returns>
    [HttpGet("health")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult> HealthCheck(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GET /api/users/health - Health check");

        // Test mediator with a simple query
        var users = await _mediator.Send(new GetAllUsersQuery(), cancellationToken);

        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            mediator = "working",
            totalUsers = users.Count,
            features = new[]
            {
                "✅ Request/Response Pattern",
                "✅ Command Pattern", 
                "✅ Query Pattern",
                "✅ Notification Pattern",
                "✅ Pipeline Behaviors (Logging, Performance, Validation)",
                "✅ Dependency Injection Integration",
                "✅ Exception Handling"
            }
        });
    }
}
