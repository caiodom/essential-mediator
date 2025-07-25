# EssentialMediator

Core implementation of EssentialMediator - Contains the main mediator implementation.

## What's Included

- `Mediator` - Main mediator implementation
- `MediatorExceptions` - Custom exceptions for better debugging

## Purpose

This package contains the core implementation of EssentialMediator with:

- **Main mediator logic**
- **Custom exceptions** for better debugging experience
- **Lightweight** - minimal dependencies

## Usage

This package is typically used together with:

- **EssentialMediator.Abstractions** - For interfaces (auto-referenced)
- **EssentialMediator.Extensions.DependencyInjection** - For registration

```csharp
// Direct usage (not recommended - use DI extensions instead)
var services = new ServiceCollection();
services.AddLogging();
services.AddSingleton<IMediator, Mediator>();

var mediator = serviceProvider.GetRequiredService<IMediator>();
var result = await mediator.Send(new GetUserQuery { Id = 1 });
```

## Related Packages

- **EssentialMediator.Abstractions** - Core interfaces
- **EssentialMediator.Extensions.DependencyInjection** - DI registration extensions
