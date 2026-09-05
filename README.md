# EssentialMediator

[![MIT License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

A small mediator implementation for .NET 10 with typed request/response dispatch, notifications, pipeline behaviors, and optional Microsoft dependency-injection integration.

> **Status:** active development. The public API is usable, but the project has not published an official GitHub release yet.

## Why EssentialMediator

EssentialMediator keeps the contracts, runtime, and Microsoft DI integration in separate projects so applications can depend only on the layer they need.

- **Typed requests and responses** with `IRequest<TResponse>` and `IRequestHandler<TRequest, TResponse>`
- **Void-style commands** with `IRequest` and `IRequestHandler<TRequest>`
- **Notifications** with multiple handlers executed concurrently
- **Pipeline behaviors** for cross-cutting concerns
- **Built-in logging, DataAnnotations validation, and performance monitoring behaviors**
- **Container-neutral runtime** based on `IServiceProvider`
- **Microsoft DI integration** with assembly scanning and configurable lifetimes
- **Fail-fast registration diagnostics** when an assembly cannot be scanned correctly
- **Typed dispatch-wrapper caching** without `MethodInfo.Invoke` in the request/notification hot path
- **Reproducible BenchmarkDotNet suite** for dispatch overhead and allocations

## Project structure

| Project | Purpose |
| --- | --- |
| `EssentialMediator.Abstractions` | Interfaces, message contracts, handlers, pipeline contracts, and `Unit` |
| `EssentialMediator` | Mediator runtime, typed dispatch wrappers, built-in behaviors, and runtime exceptions |
| `EssentialMediator.Extensions.DependencyInjection` | Microsoft DI registration, assembly scanning, and lifetime configuration |
| `EssentialMediator.Tests` | Unit and regression tests |
| `EssentialMediator.Benchmarks` | BenchmarkDotNet performance and allocation benchmarks |
| `EssentialMediator.WebApiDemo` | Example ASP.NET Core application |

## Requirements

- .NET 10 SDK

Until an official package release is published, build or reference the projects from source.

```bash
git clone https://github.com/caiodom/essential-mediator.git
cd essential-mediator
dotnet build EssentialMediator.sln -c Release
```

## Registration with Microsoft DI

### Basic registration

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

### Independent lifetimes

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

`WithServiceLifetime(...)` is retained only as an obsolete compatibility API and configures both lifetimes together. New code should use `WithHandlerLifetime(...)` and `WithMediatorLifetime(...)`.

## Request/response

Define a request:

```csharp
using EssentialMediator.Abstractions.Messages;

public sealed record GetUserQuery(int UserId) : IRequest<User>;
```

Create exactly one handler for that request:

```csharp
using EssentialMediator.Abstractions.Handlers;

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

Dispatch through `IMediator`:

```csharp
var user = await mediator.Send(
    new GetUserQuery(42),
    cancellationToken);
```

If no request handler is registered, `HandlerNotFoundException` is thrown. If more than one handler is registered for a request, `MultipleHandlersException` is thrown.

## Commands without a response value

Use `IRequest` when the operation only needs completion semantics:

```csharp
public sealed record DeleteUserCommand(int UserId) : IRequest;

public sealed class DeleteUserHandler : IRequestHandler<DeleteUserCommand>
{
    private readonly IUserRepository _users;

    public DeleteUserHandler(IUserRepository users)
    {
        _users = users;
    }

    public async Task<Unit> Handle(
        DeleteUserCommand request,
        CancellationToken cancellationToken = default)
    {
        await _users.DeleteAsync(request.UserId, cancellationToken);
        return Unit.Value;
    }
}

await mediator.Send(new DeleteUserCommand(42), cancellationToken);
```

## Notifications

A notification can have zero, one, or many handlers:

```csharp
public sealed record UserCreated(int UserId) : INotification;

public sealed class AuditUserCreated : INotificationHandler<UserCreated>
{
    public Task Handle(
        UserCreated notification,
        CancellationToken cancellationToken = default)
    {
        // Audit work.
        return Task.CompletedTask;
    }
}

public sealed class SendWelcomeEmail : INotificationHandler<UserCreated>
{
    public Task Handle(
        UserCreated notification,
        CancellationToken cancellationToken = default)
    {
        // Email work.
        return Task.CompletedTask;
    }
}

await mediator.Publish(new UserCreated(42), cancellationToken);
```

Notification handlers are started without serially awaiting each handler and are then awaited together. Synchronous handler failures, asynchronous handler failures, and cancellation are propagated to the caller rather than being silently swallowed.

## Pipeline behaviors

Pipeline behaviors wrap request handling and execute in registration order.

```csharp
services
    .AddEssentialMediator(typeof(Program).Assembly)
    .AddLoggingBehavior()
    .AddPerformanceBehavior(slowRequestThresholdMs: 500)
    .AddValidationBehavior();
```

Or register all built-in behaviors:

```csharp
services
    .AddEssentialMediator(typeof(Program).Assembly)
    .AddAllBuiltInBehaviors(slowRequestThresholdMs: 500);
```

### Built-in validation

The built-in `ValidationBehavior<,>` uses `System.ComponentModel.DataAnnotations`. Validation is **not automatic unless the validation behavior is registered**.

```csharp
using System.ComponentModel.DataAnnotations;

public sealed record CreateUserCommand(
    [property: Required] string Name,
    [property: EmailAddress] string Email) : IRequest<int>;
```

Invalid requests cause `System.ComponentModel.DataAnnotations.ValidationException` before the request handler executes.

FluentValidation is used by the Web API sample, but it is not the implementation behind EssentialMediator's built-in validation behavior. Applications that prefer FluentValidation can implement their own `IPipelineBehavior<TRequest, TResponse>`.

### Performance monitoring

The built-in performance behavior measures request duration and logs a warning when the configured threshold is exceeded:

```csharp
services.AddPerformanceBehavior(slowRequestThresholdMs: 250);
```

The threshold is injected into `PerformanceBehavior<,>` through `PerformanceBehaviorOptions`; it is not a documentation-only setting.

## Container-neutral runtime

The mediator core depends on `IServiceProvider`, not on `Microsoft.Extensions.DependencyInjection` extension methods. A container only needs to resolve the corresponding `IEnumerable<TService>` registrations required by the mediator.

Microsoft DI-specific registration and assembly scanning live in `EssentialMediator.Extensions.DependencyInjection`.

## Handler discovery

The Microsoft DI integration scans public, concrete types for request and notification handler interfaces.

Assembly loading failures are fail-fast. A `ReflectionTypeLoadException` is surfaced as `MediatorRegistrationException` with the assembly identity and loader exceptions preserved, so invalid startup configuration is not converted into a misleading runtime "handler not found" error.

## Dispatch implementation

`Mediator` caches typed request and notification wrapper instances in `ConcurrentDictionary` instances.

Reflection is used only when a typed wrapper is first constructed for a message type. Normal dispatch calls the typed handler and pipeline interfaces directly; the hot path does not use `MethodInfo.Invoke`.

## Benchmarks

A BenchmarkDotNet suite lives under [`benchmarks/`](benchmarks/README.md). It compares:

- direct request-handler invocation
- `Mediator.Send`
- `Mediator.Send` with one pipeline behavior
- direct notification-handler invocation
- `Mediator.Publish`

It also enables `MemoryDiagnoser` to report allocations.

Run it locally on controlled hardware:

```bash
dotnet run -c Release \
  --project benchmarks/EssentialMediator.Benchmarks/EssentialMediator.Benchmarks.csproj
```

The repository intentionally does not publish performance numbers based on GitHub-hosted runner timings. Compare benchmark results on the same machine and runtime before drawing performance conclusions.

## Testing and quality gates

Run the tests locally:

```bash
dotnet test tests/EssentialMediator.Tests/EssentialMediator.Tests.csproj -c Release
```

Pull-request CI currently verifies:

- restore on .NET 10
- Release build with warnings treated as errors
- benchmark project compilation
- unit/regression tests
- at least **90% line coverage** and **80% branch coverage**
- successful creation of all three NuGet packages
- successful creation of `.snupkg` symbol packages

Coverage is generated with the existing `coverlet.collector` dependency and enforced from Cobertura output.

## NuGet package readiness

The three public projects contain NuGet metadata and are validated with `dotnet pack` in CI:

- `EssentialMediator.Abstractions`
- `EssentialMediator`
- `EssentialMediator.Extensions.DependencyInjection`

Package builds include portable symbols and Source Link-compatible repository metadata. An official package/release publishing flow should be performed explicitly rather than as a side effect of merging into `develop`.

## Development flow

The repository uses `develop` as the integration branch and `main` for releases. Changes should be reviewed and validated through pull requests before integration. Promotion from `develop` to `main` is explicit; the repository does not automatically create a release PR after every integration merge.

## License

Licensed under the [MIT License](LICENSE).
