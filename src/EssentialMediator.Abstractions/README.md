# EssentialMediator.Abstractions

Abstractions package for EssentialMediator - Contains core interfaces and contracts with zero dependencies.

## What's Included

- `IMediator` - Core mediator interface
- `IRequest<T>` / `IRequest` - Request contracts
- `IRequestHandler<T,R>` / `IRequestHandler<T>` - Handler contracts  
- `INotification` - Notification contract
- `INotificationHandler<T>` - Notification handler contract
- `Unit` - Void type representation

## Purpose

This package contains only interfaces and basic types with zero dependencies

## Usage

```csharp
// Reference only abstractions in your domain/application layer
public class GetUserQuery : IRequest<User>
{
    public int Id { get; set; }
}

public class GetUserHandler : IRequestHandler<GetUserQuery, User>
{
    public async Task<User> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        // Your logic here
        return await GetUserFromDatabase(request.Id);
    }
}
```

## Related Packages

- **EssentialMediator** - Core implementation
- **EssentialMediator.Extensions.DependencyInjection** - DI registration extensions
