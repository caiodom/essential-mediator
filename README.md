# EssentialMediator

[![MIT License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET 9.0](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/)

> **Under Construction** - This project is currently under active development. APIs may change and features may be incomplete. Not recommended for production use yet.

A simple, lightweight, and efficient implementation of the Mediator pattern for .NET, designed for simplicity and enhanced functionality with modular architecture.

## Features

- **Request/Response Pattern** - Type-safe request handling with automatic validation
- **Notification Pattern** - Publish events to multiple handlers (parallel execution)
- **Pipeline Behaviors** - Cross-cutting concerns (logging, validation, performance monitoring)
- **Custom Exceptions** - Clear error messages for better debugging experience
- **Modular Architecture** - Abstractions layer for clean dependency management
- **High Performance** - Optimized with caching and efficient handler resolution
- **Flexible DI** - Configurable service lifetimes and advanced configuration
- **Built-in Behaviors** - Ready-to-use logging, validation, and performance behaviors

## Projects Structure

EssentialMediator is built with a clean, modular architecture:

### Core Projects

**EssentialMediator.Abstractions** - Core interfaces and contracts (zero dependencies)
- `IMediator`, `IRequest<T>`, `INotification`
- `IRequestHandler<,>`, `INotificationHandler<>`
- `IPipelineBehavior<,>`, `Unit` type

**EssentialMediator** - Core implementation
- Optimized `Mediator` class with caching
- Custom exceptions (`HandlerNotFoundException`, `MultipleHandlersException`)
- Built-in behaviors (Logging, Validation, Performance)

**EssentialMediator.Extensions.DependencyInjection** - Microsoft DI integration
- `AddEssentialMediator()` extensions
- Advanced configuration options
- Automatic handler registration

### Benefits of Modular Design

- Reference only abstractions in your domain layer
- Keep implementation details separate
- Easy unit testing with minimal dependencies
- Clean architecture compliance

## Getting Started

### Setup from Source

Clone the repository and build the project:

```bash
git clone https://github.com/caiodom/essential-mediator.git
cd essential-mediator
dotnet build
```

### Basic Registration

```csharp
// Simple registration by scanning assemblies for handlers
services.AddEssentialMediator(typeof(Program).Assembly);

// Or scan multiple assemblies
services.AddEssentialMediator(
    typeof(Program).Assembly,
    typeof(SomeOtherAssemblyType).Assembly
);
```

### Advanced Configuration

```csharp
services.AddEssentialMediator(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<Program>()
       .RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly())
       .WithServiceLifetime(ServiceLifetime.Scoped);
});

// Add built-in behaviors
services.AddEssentialMediator(Assembly.GetExecutingAssembly())
        .AddAllBuiltInBehaviors(slowRequestThresholdMs: 500);
```

## Usage Examples

### Request/Response Pattern

```csharp
// Define a request
public class GetUserQuery : IRequest<User>
{
    public int UserId { get; set; }
}

// Create a handler
public class GetUserHandler : IRequestHandler<GetUserQuery, User>
{
    private readonly IUserRepository _userRepository;

    public GetUserHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<User> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        return await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
    }
}

// Use in controller/service
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator) => _mediator = mediator;

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(int id, CancellationToken cancellationToken)
    {
        var user = await _mediator.Send(new GetUserQuery { UserId = id }, cancellationToken);
        return Ok(user);
    }
}
```

### Commands (Void Response)

```csharp
// Define a command
public class DeleteUserCommand : IRequest
{
    public int UserId { get; set; }
}

// Create a handler
public class DeleteUserHandler : IRequestHandler<DeleteUserCommand>
{
    private readonly IUserRepository _userRepository;

    public DeleteUserHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        await _userRepository.DeleteAsync(request.UserId, cancellationToken);
        return Unit.Value; // or just return Unit.Value
    }
}

// Usage
await _mediator.Send(new DeleteUserCommand { UserId = 123 });
```

### Notifications (Events)

```csharp
// Define a notification
public class UserCreatedEvent : INotification
{
    public User User { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// Multiple handlers can handle the same notification
public class SendWelcomeEmailHandler : INotificationHandler<UserCreatedEvent>
{
    private readonly IEmailService _emailService;

    public SendWelcomeEmailHandler(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken)
    {
        await _emailService.SendWelcomeEmailAsync(notification.User.Email, cancellationToken);
    }
}

public class UpdateAuditLogHandler : INotificationHandler<UserCreatedEvent>
{
    private readonly IAuditService _auditService;

    public UpdateAuditLogHandler(IAuditService auditService)
    {
        _auditService = auditService;
    }

    public async Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken)
    {
        await _auditService.LogUserCreationAsync(notification.User, cancellationToken);
    }
}

// Usage - all handlers will be executed in parallel
await _mediator.Publish(new UserCreatedEvent { User = user });
```

## Pipeline Behaviors

Pipeline behaviors provide a powerful way to implement cross-cutting concerns that execute around your handlers:

### Built-in Behaviors

EssentialMediator includes ready-to-use behaviors:

```csharp
// Add all built-in behaviors
services.AddEssentialMediator(Assembly.GetExecutingAssembly())
        .AddAllBuiltInBehaviors(slowRequestThresholdMs: 500);

// Or add individual behaviors
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
```

### Custom Behavior Examples

#### Logging Behavior
```csharp
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        _logger.LogInformation("Handling request {RequestName}", requestName);
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var response = await next();
            stopwatch.Stop();
            
            _logger.LogInformation("Request {RequestName} handled successfully in {ElapsedMs}ms", 
                requestName, stopwatch.ElapsedMilliseconds);
            
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Request {RequestName} failed after {ElapsedMs}ms", 
                requestName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
```

#### Validation Behavior (with FluentValidation)
```csharp
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            var failures = validationResults
                .Where(r => !r.IsValid)
                .SelectMany(r => r.Errors)
                .ToList();

            if (failures.Any())
            {
                throw new ValidationException(failures);
            }
        }

        return await next();
    }
}
```

#### Performance Monitoring Behavior
```csharp
public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
    private readonly int _slowRequestThresholdMs;

    public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger, 
                             int slowRequestThresholdMs = 500)
    {
        _logger = logger;
        _slowRequestThresholdMs = slowRequestThresholdMs;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await next();
        stopwatch.Stop();

        if (stopwatch.ElapsedMilliseconds > _slowRequestThresholdMs)
        {
            var requestName = typeof(TRequest).Name;
            _logger.LogWarning("Slow request detected: {RequestName} took {ElapsedMs}ms", 
                requestName, stopwatch.ElapsedMilliseconds);
        }

        return response;
    }
}
```

## Exception Handling

EssentialMediator provides specific exceptions for better debugging and error handling:

### Custom Exceptions

```csharp
// Handler not found
public class HandlerNotFoundException : MediatorException
{
    public Type RequestType { get; }
}

// Multiple handlers registered for same request (invalid)
public class MultipleHandlersException : MediatorException
{
    public Type RequestType { get; }
}

// Handler configuration issues
public class HandlerConfigurationException : MediatorException
{
    // Configuration-specific error details
}
```

### Exception Handling Example

```csharp
try
{
    var result = await _mediator.Send(new GetUserQuery { UserId = 999 });
}
catch (HandlerNotFoundException ex)
{
    // No handler registered for GetUserQuery
    _logger.LogError("Handler not found for {RequestType}", ex.RequestType.Name);
    return NotFound($"No handler found for {ex.RequestType.Name}");
}
catch (MultipleHandlersException ex)
{
    // Multiple handlers registered for the same request (configuration error)
    _logger.LogError("Multiple handlers found for {RequestType}", ex.RequestType.Name);
    return StatusCode(500, "Server configuration error");
}
catch (ValidationException ex)
{
    // Validation failed in pipeline behavior
    return BadRequest(ex.Errors);
}
catch (Exception ex)
{
    // General exception handling
    _logger.LogError(ex, "Unexpected error occurred");
    return StatusCode(500, "Internal server error");
}
```

## CQRS Pattern Implementation

EssentialMediator is perfect for implementing Command Query Responsibility Segregation (CQRS):

### Commands (State Changes)

```csharp
// Command for creating an order
public class CreateOrderCommand : IRequest<CreateOrderResult>
{
    public string CustomerId { get; set; } = string.Empty;
    public List<OrderItem> Items { get; set; } = new();
    public string ShippingAddress { get; set; } = string.Empty;
}

public class CreateOrderResult
{
    public string OrderId { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, CreateOrderResult>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMediator _mediator;

    public CreateOrderHandler(IOrderRepository orderRepository, IMediator mediator)
    {
        _orderRepository = orderRepository;
        _mediator = mediator;
    }

    public async Task<CreateOrderResult> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = new Order
        {
            CustomerId = request.CustomerId,
            Items = request.Items,
            ShippingAddress = request.ShippingAddress,
            CreatedAt = DateTime.UtcNow
        };

        await _orderRepository.CreateAsync(order, cancellationToken);

        // Publish domain event
        await _mediator.Publish(new OrderCreatedEvent { Order = order }, cancellationToken);

        return new CreateOrderResult
        {
            OrderId = order.Id,
            TotalAmount = order.TotalAmount,
            CreatedAt = order.CreatedAt
        };
    }
}
```

### Queries (Read-Only)

```csharp
// Query for getting orders with pagination
public class GetOrdersQuery : IRequest<PagedResult<OrderDto>>
{
    public string CustomerId { get; set; } = string.Empty;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Status { get; set; }
}

public class GetOrdersHandler : IRequestHandler<GetOrdersQuery, PagedResult<OrderDto>>
{
    private readonly IOrderReadRepository _orderRepository;

    public GetOrdersHandler(IOrderReadRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<PagedResult<OrderDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetPagedAsync(
            request.CustomerId,
            request.Page,
            request.PageSize,
            request.Status,
            cancellationToken);

        return orders;
    }
}
```

### Domain Events

```csharp
// Domain event for order creation
public class OrderCreatedEvent : INotification
{
    public Order Order { get; set; } = null!;
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}

// Multiple handlers for the same event
public class SendOrderConfirmationHandler : INotificationHandler<OrderCreatedEvent>
{
    private readonly IEmailService _emailService;

    public SendOrderConfirmationHandler(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task Handle(OrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        await _emailService.SendOrderConfirmationAsync(notification.Order, cancellationToken);
    }
}

public class UpdateInventoryHandler : INotificationHandler<OrderCreatedEvent>
{
    private readonly IInventoryService _inventoryService;

    public UpdateInventoryHandler(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    public async Task Handle(OrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        await _inventoryService.ReserveItemsAsync(notification.Order.Items, cancellationToken);
    }
}
```

## Performance Features

EssentialMediator is optimized for high performance:

### Caching and Optimization

- **Handler Method Caching**: Uses `ConcurrentDictionary` to cache reflection calls
- **Efficient Service Resolution**: Optimized service provider usage
- **Parallel Notification Execution**: All notification handlers run in parallel by default
- **Memory Efficient**: Minimal allocations and GC pressure

### Performance Monitoring

```csharp
// Built-in performance behavior
services.AddEssentialMediator(Assembly.GetExecutingAssembly())
        .AddAllBuiltInBehaviors(slowRequestThresholdMs: 500);

// This will log warnings for requests taking longer than 500ms
```

## Testing

### Running Tests

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
cd tests/EssentialMediator.Tests
dotnet test
```

### Testing Handlers

```csharp
public class GetUserHandlerTests
{
    private readonly Mock<IUserRepository> _mockRepository;
    private readonly GetUserHandler _handler;

    public GetUserHandlerTests()
    {
        _mockRepository = new Mock<IUserRepository>();
        _handler = new GetUserHandler(_mockRepository.Object);
    }

    [Fact]
    public async Task Handle_ValidUserId_ReturnsUser()
    {
        // Arrange
        var userId = 1;
        var expectedUser = new User { Id = userId, Name = "Test User" };
        _mockRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(expectedUser);

        var query = new GetUserQuery { UserId = userId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(expectedUser, result);
        _mockRepository.Verify(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
```

### Testing with Mediator

```csharp
public class UserControllerTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly UsersController _controller;

    public UserControllerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _controller = new UsersController(_mockMediator.Object);
    }

    [Fact]
    public async Task GetUser_ValidId_ReturnsOkResult()
    {
        // Arrange
        var userId = 1;
        var expectedUser = new User { Id = userId, Name = "Test User" };
        _mockMediator.Setup(x => x.Send(It.IsAny<GetUserQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expectedUser);

        // Act
        var result = await _controller.GetUser(userId, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(expectedUser, okResult.Value);
    }
}
```

## Quick Start Guide

### 1. Create a New Project

```bash
mkdir MyApp
cd MyApp
dotnet new webapi
```

### 2. Add Project References

Add references to the EssentialMediator projects:

```bash
# Add reference to the core abstractions
dotnet add reference path/to/EssentialMediator.Abstractions/EssentialMediator.Abstractions.csproj

# Add reference to the main implementation  
dotnet add reference path/to/EssentialMediator/EssentialMediator.csproj

# Add reference to DI extensions
dotnet add reference path/to/EssentialMediator.Extensions.DependencyInjection/EssentialMediator.Extensions.DependencyInjection.csproj
```

### 3. Setup Program.cs

```csharp
using EssentialMediator.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add EssentialMediator
builder.Services.AddEssentialMediator(typeof(Program).Assembly)
                .AddAllBuiltInBehaviors(slowRequestThresholdMs: 500);

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
```

### 3. Create Your First Request

```csharp
// Models/GetTimeQuery.cs
public class GetTimeQuery : IRequest<GetTimeResponse>
{
    public string? TimeZone { get; set; }
}

public class GetTimeResponse
{
    public DateTime CurrentTime { get; set; }
    public string TimeZone { get; set; } = string.Empty;
}

// Handlers/GetTimeHandler.cs
public class GetTimeHandler : IRequestHandler<GetTimeQuery, GetTimeResponse>
{
    public Task<GetTimeResponse> Handle(GetTimeQuery request, CancellationToken cancellationToken)
    {
        var timeZone = request.TimeZone ?? "UTC";
        var currentTime = timeZone == "UTC" ? DateTime.UtcNow : DateTime.Now;

        return Task.FromResult(new GetTimeResponse
        {
            CurrentTime = currentTime,
            TimeZone = timeZone
        });
    }
}

// Controllers/TimeController.cs
[ApiController]
[Route("api/[controller]")]
public class TimeController : ControllerBase
{
    private readonly IMediator _mediator;

    public TimeController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<GetTimeResponse>> GetTime([FromQuery] string? timeZone)
    {
        var response = await _mediator.Send(new GetTimeQuery { TimeZone = timeZone });
        return Ok(response);
    }
}
```


## Advanced Usage

### Custom Service Lifetimes

```csharp
services.AddEssentialMediator(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<Program>()
       .WithServiceLifetime(ServiceLifetime.Transient); // or Scoped, Singleton
});
```

### Conditional Handler Registration

```csharp
services.AddEssentialMediator(cfg =>
{
    cfg.RegisterServicesFromAssemblies(
        Assembly.GetExecutingAssembly(),
        typeof(ExternalLibrary.Handler).Assembly
    );
});
```

### Integration with ASP.NET Core

```csharp
// Startup.cs or Program.cs
public void ConfigureServices(IServiceCollection services)
{
    // Database
    services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(connectionString));

    // Validation
    services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

    // Logging
    services.AddLogging(builder =>
    {
        builder.AddConsole();
        builder.AddApplicationInsights();
    });

    // EssentialMediator with all behaviors
    services.AddEssentialMediator(Assembly.GetExecutingAssembly())
            .AddAllBuiltInBehaviors(slowRequestThresholdMs: 1000);

    // Add controllers
    services.AddControllers();
}
```

### Working with Background Services

```csharp
public class OrderProcessingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OrderProcessingService> _logger;

    public OrderProcessingService(IServiceProvider serviceProvider, ILogger<OrderProcessingService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            try
            {
                await mediator.Send(new ProcessPendingOrdersCommand(), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing pending orders");
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
```

## Migration Guide

### From MediatR

EssentialMediator is designed to be largely compatible with MediatR:

```csharp
// MediatR
using MediatR;

// EssentialMediator - similar interface
using EssentialMediator.Mediation;
using EssentialMediator.Abstractions.Messages;
using EssentialMediator.Abstractions.Handlers;

// Registration
// MediatR
services.AddMediatR(typeof(Program));

// EssentialMediator
services.AddEssentialMediator(typeof(Program).Assembly);
```

### Key Differences

1. **Namespace Structure**: More organized with `.Abstractions` separation
2. **Performance**: Optimized implementation with caching
3. **Built-in Behaviors**: Ready-to-use common behaviors
4. **Exception Handling**: More specific custom exceptions
5. **Configuration**: More flexible configuration options

## Samples and Examples

Check out the complete samples in the repository:

**`samples/EssentialMediator.WebApiDemo/`** - Complete ASP.NET Core Web API example
- CRUD operations
- Validation with FluentValidation
- Pipeline behaviors
- Error handling
- Swagger documentation

Run the sample:

```bash
cd samples/EssentialMediator.WebApiDemo
dotnet run
```

Then visit `https://localhost:5001/swagger` to explore the API.

## Best Practices

### 1. Keep Handlers Simple

```csharp
// Good - focused, single responsibility
public class GetUserHandler : IRequestHandler<GetUserQuery, User>
{
    private readonly IUserRepository _repository;

    public GetUserHandler(IUserRepository repository)
    {
        _repository = repository;
    }

    public Task<User> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        return _repository.GetByIdAsync(request.UserId, cancellationToken);
    }
}

// Avoid - too much logic in handler
public class CreateUserHandler : IRequestHandler<CreateUserCommand, User>
{
    public async Task<User> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Validation logic
        // Business logic
        // Database operations
        // Email sending
        // Logging
        // ... too much responsibility
    }
}
```

### 2. Use Pipeline Behaviors for Cross-Cutting Concerns

```csharp
// Good - validation in behavior
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    // Validation logic here
}

// Good - clean handler
public class CreateUserHandler : IRequestHandler<CreateUserCommand, User>
{
    // Only business logic here
}
```

### 3. Use Meaningful Naming

```csharp
// Good
public class GetUserByIdQuery : IRequest<User> { }
public class CreateUserCommand : IRequest<CreateUserResult> { }
public class UserCreatedEvent : INotification { }

// Avoid
public class UserRequest : IRequest<User> { }
public class UserCommand : IRequest { }
public class UserEvent : INotification { }
```

### 4. Handle Cancellation Properly

```csharp
public class GetUsersHandler : IRequestHandler<GetUsersQuery, List<User>>
{
    private readonly IUserRepository _repository;

    public GetUsersHandler(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<User>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        // Pass cancellation token to async operations
        return await _repository.GetAllAsync(cancellationToken);
    }
}
```

## Contributing

Contributions are welcome! Please follow these guidelines:

### How to Contribute

1. **Fork the Project**
   ```bash
   git clone https://github.com/caiodom/essential-mediator.git
   cd essential-mediator
   ```

2. **Create a Feature Branch**
   ```bash
   git checkout -b feature/amazing-feature
   ```

3. **Make Your Changes**
   - Follow the existing code style and conventions
   - Add tests for new functionality
   - Update documentation as needed

4. **Run Tests**
   ```bash
   dotnet test
   ```

5. **Commit Your Changes**
   ```bash
   git commit -m 'Add some amazing feature'
   ```

6. **Push to the Branch**
   ```bash
   git push origin feature/amazing-feature
   ```

7. **Open a Pull Request**
   - Provide a clear description of the changes
   - Link any related issues
   - Ensure all checks pass

### Development Guidelines

- **Code Style**: Follow standard C# conventions and use EditorConfig
- **Testing**: Maintain or improve test coverage
- **Documentation**: Update README and XML documentation
- **Performance**: Consider performance implications of changes
- **Breaking Changes**: Avoid breaking changes in minor versions

### Areas for Contribution

- Bug fixes
- Performance improvements
- Documentation improvements
- Additional test cases
- New pipeline behaviors
- Integration packages

## License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

### MIT License Summary

- **Commercial use** - Use in commercial projects
- **Modification** - Modify the source code
- **Distribution** - Distribute copies
- **Private use** - Use privately
- **Liability** - No warranty or liability
- **Warranty** - No warranty provided

---

## Links

- **Repository**: [https://github.com/caiodom/essential-mediator](https://github.com/caiodom/essential-mediator)
- **Issues**: [https://github.com/caiodom/essential-mediator/issues](https://github.com/caiodom/essential-mediator/issues)
- **Discussions**: [https://github.com/caiodom/essential-mediator/discussions](https://github.com/caiodom/essential-mediator/discussions)

---

**Made with care by [Caio Henrique Domingues Leite](https://github.com/caiodom)**


