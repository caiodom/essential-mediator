# EssentialMediator

[![MIT License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET 9.0](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/)

A **simple, and efficient** implementation of the Mediator pattern for .NET designed for simplicity and enhanced functionality.

## Features

-  **Request/Response Pattern** - Type-safe request handling
-  **Notification Pattern** - Publish to multiple handlers
-  **Pipeline Behaviors** - Cross-cutting concerns (logging, validation, performance)
-  **Custom Exceptions** - Better debugging experience
-  **Flexible DI** - Configurable service lifetimes

##  Installation and Setup

### Installation

```bash
dotnet add package EssentialMediator
```

### Registration

```csharp
// Simple registration by scanning assemblies for handlers
services.AddEssentialMediator(typeof(Program).Assembly);

// Or with configuration
services.AddEssentialMediator(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<Program>();
    cfg.WithServiceLifetime(ServiceLifetime.Scoped);
});
```

##  Usage Examples

### Request/Response

```csharp
// Define a request
public class GetUserQuery : IRequest<User>
{
    public int UserId { get; set; }
}

// Create a handler
public class GetUserHandler : IRequestHandler<GetUserQuery, User>
{
    public async Task<User> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        // Your logic here
        return await _userRepository.GetByIdAsync(request.UserId);
    }
}

// Use in controller/service
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserController(IMediator mediator) => _mediator = mediator;

    [HttpGet("{id}")]
    public async Task<User> GetUser(int id)
    {
        return await _mediator.Send(new GetUserQuery { UserId = id });
    }
}
```

### Commands (no response)

```csharp
public class DeleteUserCommand : IRequest
{
    public int UserId { get; set; }
}

public class DeleteUserHandler : IRequestHandler<DeleteUserCommand>
{
    public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        await _userRepository.DeleteAsync(request.UserId);
        return Unit.Value;
    }
}
```




### Notifications

```csharp
public class UserCreatedEvent : INotification
{
    public User User { get; set; }
}

public class SendWelcomeEmailHandler : INotificationHandler<UserCreatedEvent>
{
    public async Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken)
    {
        // Send welcome email
        await SendWelcomeEmail(notification.User.Email);
    }
}

public class UpdateAuditLogHandler : INotificationHandler<UserCreatedEvent>
{
    public async Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken)
    {
        // Update audit log
        await LogUserCreation(notification.User);
    }
}

// Usage
await _mediator.Publish(new UserCreatedEvent { User = user });
```

## Pipeline Behaviors

Pipeline behaviors allow implementing cross-cutting concerns elegantly:

### Custom Behavior Example

```csharp
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        _logger.LogInformation("Handling {RequestName}", requestName);
        
        var stopwatch = Stopwatch.StartNew();
        var response = await next();
        stopwatch.Stop();
        
        _logger.LogInformation("Handled {RequestName} in {ElapsedMs}ms", requestName, stopwatch.ElapsedMilliseconds);
        
        return response;
    }
}

// Registration
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
```

### Validation Behavior

```csharp
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();
        
        if (!Validator.TryValidateObject(request, context, results, true))
        {
            var errors = string.Join(", ", results.Select(r => r.ErrorMessage));
            throw new ValidationException($"Validation failed: {errors}");
        }
        
        return await next();
    }
}
```

## Custom Exceptions

EssentialMediator provides custom exceptions for better debugging:

```csharp
try
{
    await _mediator.Send(new GetUserQuery { UserId = 999 });
}
catch (HandlerNotFoundException ex)
{
    // No handler registered for GetUserQuery
    Console.WriteLine(ex.Message);
}
catch (MultipleHandlersException ex)
{
    // Multiple handlers registered for the same request
    Console.WriteLine(ex.Message);
}
catch (HandlerConfigurationException ex)
{
    // Handler configuration error
    Console.WriteLine(ex.Message);
}
```

## Running the Project

### Tests

```bash
cd tests/EssentialMediator.Tests
dotnet test
```

## CQRS Pattern

EssentialMediator is perfect for implementing CQRS:

```csharp
// Commands (modify state)
public class CreateOrderCommand : IRequest<Result<Order>>
{
    public string CustomerId { get; set; }
    public List<OrderItem> Items { get; set; }
}

// Queries (read-only)
public class GetOrdersQuery : IRequest<PagedResult<Order>>
{
    public string CustomerId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

// Domain Events
public class OrderCreatedEvent : INotification
{
    public Order Order { get; set; }
}
```

#### Notifications

**EssentialMediator:**
```csharp
// Parallel execution (default)
await mediator.Publish(notification);

```

**MediatR:**
```csharp
// Parallel execution (default)
await mediator.Publish(notification);

// For sequential execution, need to implement custom strategy
```

## Architecture

EssentialMediator is built with a modular architecture:

###  Packages

- **`EssentialMediator.Abstractions`** - Core interfaces and contracts (zero dependencies)
- **`EssentialMediator`** - Core implementation with Result Pattern and custom exceptions
- **`EssentialMediator.Extensions.DependencyInjection`** - Microsoft.Extensions.DependencyInjection integration

This modular approach allows you to:
- Reference only the abstractions in your domain layer
- Keep implementation details separate
- Easy unit testing with minimal dependencies

## Testing

Run the tests:

```bash
dotnet test
```


## Contributing

Contributions are welcome! Please:

1. Fork the project
2. Create a feature branch (`git checkout -b feature/new-feature`)
3. Commit your changes (`git commit -m 'Add new feature'`)
4. Push to the branch (`git push origin feature/new-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.


