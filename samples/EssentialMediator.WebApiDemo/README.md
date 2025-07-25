# EssentialMediator WebAPI Demo

> **Under Construction** - This demo project is currently under active development and may have incomplete features.

A comprehensive ASP.NET Core Web API demonstration of EssentialMediator features, showcasing real-world usage patterns and best practices.

## Features Demonstrated

- **CRUD Operations** - Complete user management with Create, Read, Update, Delete
- **Request/Response Pattern** - Queries and commands with different response types
- **Notification Pattern** - Domain events with multiple handlers
- **Pipeline Behaviors** - Logging, validation, and performance monitoring
- **Exception Handling** - Custom exception handling with proper HTTP responses
- **FluentValidation Integration** - Automatic request validation
- **Entity Framework Integration** - In-memory database for demo purposes
- **Swagger Documentation** - Interactive API documentation

## Project Structure

```
├── Controllers/
│   └── UsersController.cs          # REST API endpoints
├── Features/
│   └── Users/                      # Feature-based organization
│       ├── Commands/               # State-changing operations
│       ├── Queries/                # Data retrieval operations
│       ├── Events/                 # Domain events
│       └── Validators/             # Request validation rules
├── Data/
│   └── DemoDbContext.cs           # Entity Framework context
├── Models/
│   └── User.cs                    # Domain models
├── Middleware/
│   └── ExceptionHandlingMiddleware.cs # Global exception handling
└── Program.cs                      # Application configuration
```

## Getting Started

### 1. Run the Application

```bash
cd samples/EssentialMediator.WebApiDemo
dotnet run
```

### 2. Open Swagger UI

Navigate to `https://localhost:5001/swagger` to explore the interactive API documentation.

### 3. Test the API

Use the included `api-tests.http` file with your favorite HTTP client (VS Code REST Client, Postman, etc.)

## API Endpoints

### Users Management

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/users` | Get all users with pagination |
| GET | `/api/users/{id}` | Get user by ID |
| POST | `/api/users` | Create new user |
| PUT | `/api/users/{id}` | Update existing user |
| DELETE | `/api/users/{id}` | Delete user |

## Code Examples

### Query Example

```csharp
// Query: Get user by ID
public class GetUserByIdQuery : IRequest<UserDto>
{
    public int UserId { get; set; }
}

public class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, UserDto>
{
    private readonly DemoDbContext _context;

    public GetUserByIdHandler(DemoDbContext context)
    {
        _context = context;
    }

    public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FindAsync(request.UserId, cancellationToken);
        
        if (user == null)
            throw new NotFoundException($"User with ID {request.UserId} not found");

        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email
        };
    }
}
```

### Command Example

```csharp
// Command: Create new user
public class CreateUserCommand : IRequest<CreateUserResult>
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class CreateUserHandler : IRequestHandler<CreateUserCommand, CreateUserResult>
{
    private readonly DemoDbContext _context;
    private readonly IMediator _mediator;

    public CreateUserHandler(DemoDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task<CreateUserResult> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        // Publish domain event
        await _mediator.Publish(new UserCreatedEvent { User = user }, cancellationToken);

        return new CreateUserResult
        {
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email
        };
    }
}
```

### Event Example

```csharp
// Domain Event: User created
public class UserCreatedEvent : INotification
{
    public User User { get; set; } = null!;
}

// Event Handler: Send welcome email
public class SendWelcomeEmailHandler : INotificationHandler<UserCreatedEvent>
{
    private readonly ILogger<SendWelcomeEmailHandler> _logger;

    public SendWelcomeEmailHandler(ILogger<SendWelcomeEmailHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Sending welcome email to {Email}", notification.User.Email);
        
        // In a real application, you would send an actual email here
        // await _emailService.SendWelcomeEmailAsync(notification.User.Email);
        
        return Task.CompletedTask;
    }
}

// Event Handler: Create audit log
public class CreateAuditLogHandler : INotificationHandler<UserCreatedEvent>
{
    private readonly ILogger<CreateAuditLogHandler> _logger;

    public CreateAuditLogHandler(ILogger<CreateAuditLogHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating audit log for user creation: {UserId}", notification.User.Id);
        
        // In a real application, you would create an audit log entry
        // await _auditService.LogAsync($"User {notification.User.Id} created");
        
        return Task.CompletedTask;
    }
}
```

### Validation Example

```csharp
// FluentValidation validator
public class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(255);
    }
}
```

## Configuration

The application is configured in `Program.cs` with all EssentialMediator features:

```csharp
// Add EssentialMediator with all built-in behaviors
builder.Services.AddEssentialMediator(Assembly.GetExecutingAssembly())
                .AddAllBuiltInBehaviors(slowRequestThresholdMs: 500);

// Add FluentValidation
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

// Add Entity Framework
builder.Services.AddDbContext<DemoDbContext>(options =>
    options.UseInMemoryDatabase("DemoDatabase"));
```

## Performance Monitoring

The demo includes performance monitoring that logs slow requests:

```bash
# Example log output
info: EssentialMediator.Behaviors.PerformanceBehavior[0]
      Slow request detected: GetUsersQuery took 750ms
```

## Testing the API

### Using HTTP File

The included `api-tests.http` file contains sample requests:

```http
### Get all users
GET https://localhost:5001/api/users

### Create a new user
POST https://localhost:5001/api/users
Content-Type: application/json

{
    "name": "John Doe",
    "email": "john.doe@example.com"
}

### Get user by ID
GET https://localhost:5001/api/users/1

### Update user
PUT https://localhost:5001/api/users/1
Content-Type: application/json

{
    "name": "John Smith",
    "email": "john.smith@example.com"
}

### Delete user
DELETE https://localhost:5001/api/users/1
```

### Expected Responses

**Create User Response:**
```json
{
    "userId": 1,
    "name": "John Doe",
    "email": "john.doe@example.com"
}
```

**Get Users Response:**
```json
{
    "items": [
        {
            "id": 1,
            "name": "John Doe",
            "email": "john.doe@example.com",
            "createdAt": "2025-01-01T10:00:00Z"
        }
    ],
    "totalCount": 1,
    "page": 1,
    "pageSize": 10
}
```

## Key Learning Points

1. **Feature Organization** - Organize code by features rather than technical layers
2. **Handler Separation** - Keep handlers focused on single responsibilities
3. **Event-Driven Architecture** - Use domain events for loose coupling
4. **Pipeline Behaviors** - Implement cross-cutting concerns elegantly
5. **Validation Integration** - Automatic request validation with detailed error messages
6. **Exception Handling** - Global exception handling with appropriate HTTP status codes
7. **Performance Monitoring** - Built-in performance tracking and logging

## Next Steps

- Explore the code to understand the patterns
- Modify the handlers to add your own business logic
- Add new endpoints and handlers
- Experiment with custom pipeline behaviors
- Try different validation rules
- Implement additional event handlers

This demo provides a solid foundation for building production-ready applications with EssentialMediator.
