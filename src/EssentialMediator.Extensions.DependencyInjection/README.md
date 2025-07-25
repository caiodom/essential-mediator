# EssentialMediator.Extensions.DependencyInjection

> **Under Construction** - This project is currently under active development. APIs may change and features may be incomplete.

Dependency Injection extensions for EssentialMediator - Seamless integration with Microsoft.Extensions.DependencyInjection with advanced configuration options.

## What's Included

- **`AddEssentialMediator()`** - Registration extensions with multiple overloads
- **`MediatorConfiguration`** - Fluent configuration API for advanced scenarios
- **Assembly Scanning** - Automatic handler discovery with intelligent filtering
- **Service Lifetime Control** - Configure Singleton, Scoped, or Transient lifetimes
- **Built-in Behaviors** - Easy registration of logging, validation, and performance behaviors
- **Error Handling** - Graceful handling of assembly loading issues

## Quick Start

### Basic Registration

```csharp
// Register with single assembly scanning
services.AddEssentialMediator(typeof(Program).Assembly);

// Register with multiple assemblies
services.AddEssentialMediator(
    typeof(UserHandlers).Assembly,
    typeof(OrderHandlers).Assembly
);

// Register with built-in behaviors
services.AddEssentialMediator(typeof(Program).Assembly)
        .AddAllBuiltInBehaviors(slowRequestThresholdMs: 500);
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

## Configuration Options

### Assembly Registration

```csharp
services.AddEssentialMediator(cfg =>
{
    // Scan specific assemblies
    cfg.RegisterServicesFromAssemblies(assembly1, assembly2);
    
    // Scan by type (gets assembly from type)
    cfg.RegisterServicesFromAssemblyContaining<MyHandler>();
    cfg.RegisterServicesFromAssemblyContaining(typeof(MyHandler));
    
    // Service lifetime configuration
    cfg.WithServiceLifetime(ServiceLifetime.Scoped); // Default
    cfg.WithServiceLifetime(ServiceLifetime.Transient);
    cfg.WithServiceLifetime(ServiceLifetime.Singleton);
});
```

### Built-in Behaviors

```csharp
// Add all built-in behaviors with default settings
services.AddEssentialMediator(Assembly.GetExecutingAssembly())
        .AddAllBuiltInBehaviors();

// Add all built-in behaviors with custom performance threshold
services.AddEssentialMediator(Assembly.GetExecutingAssembly())
        .AddAllBuiltInBehaviors(slowRequestThresholdMs: 1000);

// Add individual behaviors
services.AddEssentialMediator(Assembly.GetExecutingAssembly())
        .AddLoggingBehavior()
        .AddValidationBehavior()
        .AddPerformanceBehavior(slowRequestThresholdMs: 500);
```

### Manual Registration

```csharp
// Register individual handlers manually (if needed)
services.AddEssentialMediator()
        .RegisterHandler<GetUserQuery, User, GetUserHandler>()
        .RegisterNotificationHandler<UserCreatedEvent, SendEmailHandler>();
```

## Features

### Automatic Handler Discovery

The extension automatically discovers and registers:

- **Request Handlers** - `IRequestHandler<TRequest, TResponse>` and `IRequestHandler<TRequest>`
- **Notification Handlers** - `INotificationHandler<TNotification>`
- **Pipeline Behaviors** - `IPipelineBehavior<TRequest, TResponse>`

### Service Lifetime Management

Configure how handlers are registered in the DI container:

```csharp
// Scoped (default) - One instance per request
cfg.WithServiceLifetime(ServiceLifetime.Scoped);

// Transient - New instance every time
cfg.WithServiceLifetime(ServiceLifetime.Transient);

// Singleton - Single instance for application lifetime
cfg.WithServiceLifetime(ServiceLifetime.Singleton);
```

### Error Handling

The extension gracefully handles common issues:

- **Missing Assemblies** - Skips assemblies that can't be loaded
- **Invalid Handlers** - Warns about handlers that don't implement interfaces correctly
- **Duplicate Registrations** - Prevents duplicate handler registrations

## Integration Examples

### ASP.NET Core Web API

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add EssentialMediator with all behaviors
builder.Services.AddEssentialMediator(Assembly.GetExecutingAssembly())
                .AddAllBuiltInBehaviors(slowRequestThresholdMs: 500);

// Add FluentValidation (if using validation behavior)
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
```

### Background Services

```csharp
// Register EssentialMediator for background services
services.AddEssentialMediator(typeof(Program).Assembly);

services.AddHostedService<OrderProcessingService>();

public class OrderProcessingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public OrderProcessingService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            
            await mediator.Send(new ProcessOrdersCommand(), stoppingToken);
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
```

### Multiple Assemblies

```csharp
// Register handlers from multiple assemblies
services.AddEssentialMediator(cfg =>
{
    cfg.RegisterServicesFromAssemblies(
        typeof(UserModule.Handlers.GetUserHandler).Assembly,    // User module
        typeof(OrderModule.Handlers.CreateOrderHandler).Assembly, // Order module
        typeof(PaymentModule.Handlers.ProcessPaymentHandler).Assembly // Payment module
    );
});
```

## Best Practices

1. **Use Assembly Scanning** - Let the extension discover handlers automatically
2. **Configure Lifetimes** - Choose appropriate service lifetimes for your handlers
3. **Add Behaviors** - Use built-in behaviors for common cross-cutting concerns
4. **Organize by Modules** - Group related handlers in separate assemblies
5. **Validate Configuration** - Test that all expected handlers are registered

## Troubleshooting

### Common Issues

**Handler Not Found Exception**
```csharp
// Ensure the assembly containing the handler is scanned
services.AddEssentialMediator(typeof(MyHandler).Assembly);
```

**Multiple Handlers Exception**
```csharp
// Check for duplicate handler registrations
// Only one handler should implement IRequestHandler<TRequest, TResponse>
```

**Performance Issues**
```csharp
// Consider service lifetimes - Singleton may be appropriate for stateless handlers
cfg.WithServiceLifetime(ServiceLifetime.Singleton);
```

## Related Projects

- **EssentialMediator.Abstractions** - Core interfaces and contracts
- **EssentialMediator** - Core implementation with built-in behaviors
    
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

## Related Projects

- **EssentialMediator.Abstractions** - Core interfaces
- **EssentialMediator** - Core implementation
