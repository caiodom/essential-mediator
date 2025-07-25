# EssentialMediator

> ⚠️ **Under Construction** - This project is currently under active development. APIs may change and features may be incomplete.

Core implementation of EssentialMediator - Contains the main mediator implementation with optimized performance and built-in behaviors.

## What's Included

- **`Mediator`** - Main mediator implementation with caching and optimization
- **Custom Exceptions** - `HandlerNotFoundException`, `MultipleHandlersException`, `HandlerConfigurationException`
- **Built-in Behaviors** - Ready-to-use logging, validation, and performance monitoring behaviors
- **Pipeline Support** - Full pipeline behavior support for cross-cutting concerns

## Features

- **High Performance** - Optimized with `ConcurrentDictionary` caching for reflection calls
- **Memory Efficient** - Minimal allocations and GC pressure
- **Parallel Execution** - Notification handlers run in parallel by default
- **Exception Handling** - Detailed custom exceptions for better debugging

## Purpose

This package contains the core implementation of EssentialMediator with:

- **Optimized mediator logic** with performance enhancements
- **Custom exceptions** for better debugging experience
- **Built-in behaviors** for common cross-cutting concerns
- **Lightweight dependencies** - only depends on abstractions and Microsoft.Extensions packages

## Built-in Behaviors

### LoggingBehavior
Logs request handling with execution time and error tracking.

### ValidationBehavior  
Integration with FluentValidation for automatic request validation.

### PerformanceBehavior
Monitors and logs slow requests based on configurable thresholds.

## Usage

This project is typically used together with:

- **EssentialMediator.Abstractions** - For interfaces (auto-referenced)
- **EssentialMediator.Extensions.DependencyInjection** - For registration (recommended)

```csharp
// Recommended: Use with DI extensions
services.AddEssentialMediator(typeof(Program).Assembly)
        .AddAllBuiltInBehaviors(slowRequestThresholdMs: 500);

// Direct usage (manual setup)
var services = new ServiceCollection();
services.AddLogging();
services.AddScoped<IMediator, Mediator>();

var mediator = serviceProvider.GetRequiredService<IMediator>();
var result = await mediator.Send(new GetUserQuery { Id = 1 });
```

## Related Projects

- **EssentialMediator.Abstractions** - Core interfaces and contracts
- **EssentialMediator.Extensions.DependencyInjection** - DI registration extensions
