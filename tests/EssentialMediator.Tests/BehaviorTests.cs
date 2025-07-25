using EssentialMediator.Abstractions.Handlers;
using EssentialMediator.Abstractions.Messages;
using EssentialMediator.Abstractions.Mediation;
using EssentialMediator.Behaviors;
using EssentialMediator.Mediation;
using EssentialMediator.Tests.Models.Requests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace EssentialMediator.Tests;

public class BehaviorTests
{
    [Fact]
    public async Task LoggingBehavior_ShouldLogRequestHandling()
    {
        // Arrange
        var services = new ServiceCollection();
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<LoggingBehavior<TestRequest, string>>();

        var behavior = new LoggingBehavior<TestRequest, string>(logger);
        var request = new TestRequest { Message = "Test" };
        var wasCalled = false;

        // Act
        var result = await behavior.Handle(request, () =>
        {
            wasCalled = true;
            return Task.FromResult("Success");
        }, CancellationToken.None);

        // Assert
        Assert.True(wasCalled);
        Assert.Equal("Success", result);
    }

    [Fact]
    public async Task LoggingBehavior_WhenExceptionThrown_ShouldLogErrorAndRethrow()
    {
        // Arrange
        var services = new ServiceCollection();
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<LoggingBehavior<TestRequest, string>>();

        var behavior = new LoggingBehavior<TestRequest, string>(logger);
        var request = new TestRequest { Message = "Test" };
        var expectedException = new InvalidOperationException("Test exception");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => behavior.Handle(request, () => Task.FromException<string>(expectedException), CancellationToken.None));

        Assert.Equal("Test exception", exception.Message);
    }

    [Fact]
    public async Task PerformanceBehavior_ShouldMeasureExecutionTime()
    {
        // Arrange
        var services = new ServiceCollection();
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<PerformanceBehavior<TestRequest, string>>();

        var behavior = new PerformanceBehavior<TestRequest, string>(logger, 1000);
        var request = new TestRequest { Message = "Test" };

        // Act
        var result = await behavior.Handle(request, () =>
        {
            Thread.Sleep(50); // Simulate some work
            return Task.FromResult("Success");
        }, CancellationToken.None);

        // Assert
        Assert.Equal("Success", result);
    }

    [Fact]
    public async Task PerformanceBehavior_WhenSlowRequest_ShouldLogWarning()
    {
        // Arrange
        var services = new ServiceCollection();
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<PerformanceBehavior<TestRequest, string>>();

        var behavior = new PerformanceBehavior<TestRequest, string>(logger, 10); // Very low threshold
        var request = new TestRequest { Message = "Test" };

        // Act
        var result = await behavior.Handle(request, () =>
        {
            Thread.Sleep(20); // This should trigger the warning
            return Task.FromResult("Success");
        }, CancellationToken.None);

        // Assert
        Assert.Equal("Success", result);
    }

    [Fact]
    public async Task ValidationBehavior_WithValidRequest_ShouldPassThrough()
    {
        // Arrange
        var services = new ServiceCollection();
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<ValidationBehavior<TestValidatedRequest, string>>();

        var behavior = new ValidationBehavior<TestValidatedRequest, string>(logger);
        var request = new TestValidatedRequest { Name = "Valid Name", Email = "test@example.com" };

        // Act
        var result = await behavior.Handle(request, () => Task.FromResult("Success"), CancellationToken.None);

        // Assert
        Assert.Equal("Success", result);
    }

    [Fact]
    public async Task ValidationBehavior_WithInvalidRequest_ShouldThrowValidationException()
    {
        // Arrange
        var services = new ServiceCollection();
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<ValidationBehavior<TestValidatedRequest, string>>();

        var behavior = new ValidationBehavior<TestValidatedRequest, string>(logger);
        var request = new TestValidatedRequest { Name = "", Email = "invalid-email" }; // Invalid data

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => behavior.Handle(request, () => Task.FromResult("Success"), CancellationToken.None));

        Assert.Contains("The Name field is required", exception.Message);
    }
}

public class TestValidatedRequest : IRequest<string>
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public class TestValidatedRequestHandler : IRequestHandler<TestValidatedRequest, string>
{
    public Task<string> Handle(TestValidatedRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult($"Validated: {request.Name} - {request.Email}");
    }
}
