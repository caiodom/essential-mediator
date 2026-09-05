# EssentialMediator.Abstractions

Contracts and message types for EssentialMediator on .NET 10.

Use this package when a project needs to define mediator requests, notifications, handlers, or pipeline contracts without taking a dependency on the mediator runtime or Microsoft dependency injection.

## What this package contains

- `IMediator`
- `IRequest<TResponse>` and `IRequest`
- `IRequestHandler<TRequest, TResponse>` and `IRequestHandler<TRequest>`
- `INotification` and `INotificationHandler<TNotification>`
- `IPipelineBehavior<TRequest, TResponse>`
- `RequestHandlerDelegate<TResponse>`
- `Unit`

The package has no NuGet dependencies.

## Example

```csharp
using EssentialMediator.Abstractions.Handlers;
using EssentialMediator.Abstractions.Messages;

public sealed record GetUserQuery(int UserId) : IRequest<User>;

public sealed class GetUserHandler : IRequestHandler<GetUserQuery, User>
{
    private readonly IUserRepository _users;

    public GetUserHandler(IUserRepository users)
    {
        _users = users;
    }

    public Task<User> Handle(
        GetUserQuery request,
        CancellationToken cancellationToken = default)
        => _users.GetByIdAsync(request.UserId, cancellationToken);
}
```

Commands that only need completion semantics can implement `IRequest` and return `Unit` from their handler:

```csharp
public sealed record DeleteUserCommand(int UserId) : IRequest;

public sealed class DeleteUserHandler : IRequestHandler<DeleteUserCommand>
{
    public Task<Unit> Handle(
        DeleteUserCommand request,
        CancellationToken cancellationToken = default)
    {
        // Perform the command.
        return Task.FromResult(Unit.Value);
    }
}
```

Notifications may have zero, one, or many handlers:

```csharp
public sealed record UserCreated(int UserId) : INotification;

public sealed class AuditUserCreated : INotificationHandler<UserCreated>
{
    public Task Handle(
        UserCreated notification,
        CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
```

## Related packages

- `EssentialMediator` provides the mediator runtime and built-in behaviors.
- `EssentialMediator.Extensions.DependencyInjection` provides Microsoft DI registration and assembly scanning.

For the complete API overview, examples, quality gates, changelog, and release policy, see the [EssentialMediator repository](https://github.com/caiodom/essential-mediator).
