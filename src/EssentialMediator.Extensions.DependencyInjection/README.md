# EssentialMediator.Extensions.DependencyInjection

Dependency Injection extensions for EssentialMediator - Seamless integration with Microsoft.Extensions.DependencyInjection.

## What's Included

- `AddEssentialMediator()` - Registration extensions
- `MediatorConfiguration` - Fluent configuration API
- **Assembly scanning** - Automatic handler discovery
- **Service lifetime control** - Singleton, Scoped, Transient
- **Graceful error handling** - Assembly loading issues handled

## Quick Start

### Basic Registration

```csharp
// Register with assembly scanning
services.AddEssentialMediator(typeof(Program).Assembly);

// Register with multiple assemblies
services.AddEssentialMediator(
    typeof(UserHandlers).Assembly,
    typeof(OrderHandlers).Assembly
);
```

### Advanced Configuration

```csharp
services.AddEssentialMediator(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<Program>()
       .RegisterServicesFromAssemblyContaining<UserHandler>()
       .WithServiceLifetime(ServiceLifetime.Scoped);
});
```

### Configuration Options

```csharp
services.AddEssentialMediator(cfg =>
{
    // Scan specific assemblies
    cfg.RegisterServicesFromAssemblies(assembly1, assembly2);
    
    // Scan by type
    cfg.RegisterServicesFromAssemblyContaining<MyHandler>();
    cfg.RegisterServicesFromAssemblyContaining(typeof(MyHandler));
    
    // Scan entry/calling assembly
    cfg.RegisterServicesFromEntryAssembly();
    cfg.RegisterServicesFromCallingAssembly();
    
    // Set service lifetime
    cfg.WithServiceLifetime(ServiceLifetime.Singleton);
});
```

## Service Lifetimes

| Lifetime | Usage | When to Use |
|----------|-------|-------------|
| **Scoped** (default) | One instance per request | Web APIs, typical usage |
| **Singleton** | Single instance | Stateless handlers, performance critical |
| **Transient** | New instance each time | Handlers with dependencies |

## What Gets Registered

- `IMediator` → `Mediator`
- All `IRequestHandler<,>` implementations
- All `IRequestHandler<>` implementations  
- All `INotificationHandler<>` implementations
- Handles reflection exceptions gracefully

## Requirements

- .NET 9.0+
- Microsoft.Extensions.DependencyInjection.Abstractions 8.0+

## Related Packages

- **EssentialMediator.Abstractions** - Core interfaces
- **EssentialMediator** - Core implementation
