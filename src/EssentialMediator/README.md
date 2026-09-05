# EssentialMediator

Mediator runtime for .NET 10.

This package provides the `Mediator` implementation, typed dispatch wrappers, runtime exceptions, and built-in pipeline behaviors. It references `EssentialMediator.Abstractions` automatically.

## What this package contains

- `Mediator` implementation of `IMediator`
- typed cached request and notification dispatch wrappers
- `HandlerNotFoundException`
- `MultipleHandlersException`
- `HandlerConfigurationException`
- logging pipeline behavior
- performance pipeline behavior
- DataAnnotations validation pipeline behavior

## Dispatch behavior

Request dispatch resolves exactly one handler for a request. Missing handlers and multiple handlers fail explicitly.

Typed dispatch wrappers are cached per message contract. Reflection is used when a wrapper is first created; normal request and notification dispatch calls the typed interfaces directly and does not use `MethodInfo.Invoke` in the hot path.

Notification handlers are started without serially awaiting each handler and are then awaited together. Synchronous failures, asynchronous failures, and cancellation are propagated to the caller.

## Built-in behaviors

### Logging

`LoggingBehavior<TRequest, TResponse>` logs request execution and failures through `Microsoft.Extensions.Logging`.

### Performance

`PerformanceBehavior<TRequest, TResponse>` measures request duration and logs a warning when the configured slow-request threshold is exceeded.

The threshold is supplied through `PerformanceBehaviorOptions`. When using the Microsoft DI integration, configure it with:

```csharp
services.AddPerformanceBehavior(slowRequestThresholdMs: 250);
```

### Validation

`ValidationBehavior<TRequest, TResponse>` uses `System.ComponentModel.DataAnnotations`.

It does **not** use FluentValidation. Applications that prefer FluentValidation can implement a custom `IPipelineBehavior<TRequest, TResponse>`.

## Recommended registration

For most applications, install `EssentialMediator.Extensions.DependencyInjection` and register the runtime through Microsoft DI:

```csharp
using EssentialMediator.Extensions;

services
    .AddEssentialMediator(typeof(Program).Assembly)
    .AddLoggingBehavior()
    .AddPerformanceBehavior(slowRequestThresholdMs: 500)
    .AddValidationBehavior();
```

The mediator runtime itself depends on `IServiceProvider`; Microsoft DI-specific assembly scanning and registration live in the separate DI extensions package.

## Performance

A reproducible BenchmarkDotNet suite is included in the repository. The project intentionally does not publish performance claims based on GitHub-hosted runner timings; compare results on the same hardware and runtime before drawing conclusions.

## Related packages

- `EssentialMediator.Abstractions` contains the contracts and message types.
- `EssentialMediator.Extensions.DependencyInjection` provides Microsoft DI registration and assembly scanning.

For the complete API overview, examples, benchmarks, changelog, and release policy, see the [EssentialMediator repository](https://github.com/caiodom/essential-mediator).
