# EssentialMediator.Abstractions

> **Under Construction** - This project is currently under active development. APIs may change and features may be incomplete.

Abstractions package for EssentialMediator - Contains core interfaces and contracts with zero dependencies, perfect for clean architecture implementations.

## What's Included

### Core Interfaces
- **`IMediator`** - Core mediator interface for sending requests and publishing notifications
- **`IRequest<T>`** / **`IRequest`** - Request contracts for queries and commands
- **`IRequestHandler<T,R>`** / **`IRequestHandler<T>`** - Handler contracts for processing requests
- **`INotification`** - Notification contract for events
- **`INotificationHandler<T>`** - Notification handler contract for event processing
- **`IPipelineBehavior<T,R>`** - Pipeline behavior contract for cross-cutting concerns

### Supporting Types
- **`Unit`** - Void type representation for commands without return values
- **`RequestHandlerDelegate<T>`** - Delegate for pipeline behavior chaining

## Purpose

This package contains **only interfaces and basic types** with **zero dependencies**, making it perfect for:

- **Domain Layer** - Reference only abstractions without implementation dependencies
- **Application Layer** - Define application services and handlers
- **Clean Architecture** - Maintain dependency inversion principles
- **Testing** - Easy mocking and unit testing

## Usage Examples

### Define Requests and Handlers

```csharp
// Query (returns data)
public class GetUserQuery : IRequest<User>
{
    public int UserId { get; set; }
}

public class GetUserHandler : IRequestHandler<GetUserQuery, User>
{
    public async Task<User> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        // Your business logic here
        return await GetUserFromDatabase(request.UserId);
    }
}

// Command (modifies state)
public class CreateUserCommand : IRequest<CreateUserResult>
{
    public string Name { get; set; }
    public string Email { get; set; }
}

public class CreateUserHandler : IRequestHandler<CreateUserCommand, CreateUserResult>
{
    public async Task<CreateUserResult> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Your business logic here
        var user = await CreateUser(request);
        return new CreateUserResult { UserId = user.Id };
    }
}

// Void Command (no return value)
public class DeleteUserCommand : IRequest
{
    public int UserId { get; set; }
}

public class DeleteUserHandler : IRequestHandler<DeleteUserCommand>
{
    public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        await DeleteUser(request.UserId);
        return Unit.Value;
    }
}
```

### Define Notifications and Handlers

```csharp
// Event/Notification
public class UserCreatedEvent : INotification
{
    public User User { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Multiple handlers can handle the same notification
public class SendWelcomeEmailHandler : INotificationHandler<UserCreatedEvent>
{
    public async Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken)
    {
        await SendWelcomeEmail(notification.User.Email);
    }
}

public class CreateAuditLogHandler : INotificationHandler<UserCreatedEvent>
{
    public async Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken)
    {
        await CreateAuditLog($"User {notification.User.Id} created");
    }
}
```

### Define Pipeline Behaviors

```csharp
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Pre-processing
        Console.WriteLine($"Handling {typeof(TRequest).Name}");
        
        // Call next behavior/handler
        var response = await next();
        
        // Post-processing
        Console.WriteLine($"Handled {typeof(TRequest).Name}");
        
        return response;
    }
}
```

## Benefits

- **Zero Dependencies** - No external package dependencies
- **Clean Architecture** - Perfect for domain and application layers
- **Testability** - Easy to mock and unit test
- **Flexibility** - Use with any DI container or implementation
- **Performance** - Minimal overhead, just contracts

## Related Projects

- **EssentialMediator** - Core implementation with built-in behaviors
- **EssentialMediator.Extensions.DependencyInjection** - Microsoft DI registration extensions
