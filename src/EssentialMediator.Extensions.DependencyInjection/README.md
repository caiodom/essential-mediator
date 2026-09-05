# EssentialMediator.Extensions.DependencyInjection

Microsoft.Extensions.DependencyInjection integration for EssentialMediator on .NET 10.

Install this package when you want assembly scanning, `IServiceCollection` registration, independent mediator/handler lifetime configuration, and convenient registration of the built-in pipeline behaviors. It references the `EssentialMediator` runtime and `EssentialMediator.Abstractions` transitively.

## Quick start

```csharp
using EssentialMediator.Extensions;

services.AddEssentialMediator(typeof(Program).Assembly);
```

Multiple assemblies can be scanned explicitly:

```csharp
services.AddEssentialMediator(
    typeof(Program).Assembly,
    typeof(ApplicationAssemblyMarker).Assembly);
```

## Advanced configuration

Mediator and handler lifetimes are configured independently:

```csharp
using EssentialMediator.Extensions.DependencyInjection.Configuration;
using Microsoft.Extensions.DependencyInjection;

services.AddEssentialMediator(config =>
{
    config
        .RegisterServicesFromAssemblyContaining<Program>()
        .WithHandlerLifetime(ServiceLifetime.Scoped)
        .WithMediatorLifetime(ServiceLifetime.Scoped);
});
```

Handler lifetime defaults to `Scoped` and may be `Scoped`, `Transient`, or `Singleton` when that is appropriate for the handler's own dependencies and state.

Mediator lifetime defaults to `Scoped`. `Scoped` and `Transient` are supported. `Singleton` mediator registration is rejected because the mediator resolves handlers and pipeline behaviors from the ambient service scope.

`WithServiceLifetime(...)` remains only as an obsolete compatibility API. New code should use `WithHandlerLifetime(...)` and `WithMediatorLifetime(...)`.

## Assembly scanning

The integration discovers public, concrete implementations of:

- `IRequestHandler<TRequest, TResponse>`
- `IRequestHandler<TRequest>`
- `INotificationHandler<TNotification>`

Assembly loading problems are fail-fast. A `ReflectionTypeLoadException` is surfaced as `MediatorRegistrationException` with loader exception details instead of silently skipping broken types.

## Built-in behaviors

Register all built-in behaviors:

```csharp
services
    .AddEssentialMediator(typeof(Program).Assembly)
    .AddAllBuiltInBehaviors(slowRequestThresholdMs: 500);
```

Or register them individually:

```csharp
services
    .AddEssentialMediator(typeof(Program).Assembly)
    .AddLoggingBehavior()
    .AddPerformanceBehavior(slowRequestThresholdMs: 500)
    .AddValidationBehavior();
```

The built-in validation behavior uses `System.ComponentModel.DataAnnotations`. It is not backed by FluentValidation.

## ASP.NET Core example

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddEssentialMediator(typeof(Program).Assembly)
    .AddAllBuiltInBehaviors(slowRequestThresholdMs: 500);

var app = builder.Build();
app.Run();
```

## Background services

When using scoped handlers from a hosted/background service, create a scope and resolve `IMediator` from that scope:

```csharp
using var scope = serviceProvider.CreateScope();
var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
await mediator.Send(new ProcessOrdersCommand(), stoppingToken);
```

## Registration semantics

- exactly one request handler is expected for each request contract;
- notifications may have zero, one, or many handlers;
- duplicate request-handler registrations are detected at dispatch time and fail explicitly;
- scanning the same assembly more than once is de-duplicated by the configuration path.

## Related packages

- `EssentialMediator.Abstractions` contains contracts and message types.
- `EssentialMediator` contains the runtime and built-in behaviors.

For the complete API overview, examples, quality gates, changelog, security policy, and release process, see the [EssentialMediator repository](https://github.com/caiodom/essential-mediator).
