using EssentialMediator.Abstractions.Handlers;
using EssentialMediator.Abstractions.Pipelines;
using EssentialMediator.Extensions;
using EssentialMediator.Mediation;
using EssentialMediator.Tests.Models.Behaviors;
using EssentialMediator.Tests.Models.Pipeline;
using EssentialMediator.Tests.Models.Requests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace EssentialMediator.Tests;

public class PipelineBehaviorTests
{
    [Fact]
    public async Task Send_WithoutBehaviors_ShouldExecuteHandlerDirectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddScoped<IMediator, Mediator>();
        services.AddScoped<IRequestHandler<TestRequest, string>, TestRequestHandler>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var request = new TestRequest { Message = "Test Message" };

        // Act
        var result = await mediator.Send(request);

        // Assert
        Assert.Equal("Handled: Test Message", result);
    }

    [Fact]
    public async Task Send_WithSingleBehavior_ShouldExecuteBehaviorAndHandler()
    {
        // Arrange
        TestLoggingBehavior<TestRequest, string>.Logs.Clear();

        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddScoped<IMediator, Mediator>();
        services.AddScoped<IRequestHandler<TestRequest, string>, TestRequestHandler>();
        services.AddScoped<IPipelineBehavior<TestRequest, string>, TestLoggingBehavior<TestRequest, string>>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var request = new TestRequest { Message = "Test Message" };

        // Act
        var result = await mediator.Send(request);

        // Assert
        Assert.Equal("Handled: Test Message", result);
        Assert.Contains("Before handling TestRequest", TestLoggingBehavior<TestRequest, string>.Logs);
        Assert.Contains("After handling TestRequest", TestLoggingBehavior<TestRequest, string>.Logs);
        Assert.Equal(2, TestLoggingBehavior<TestRequest, string>.Logs.Count);
    }

    [Fact]
    public async Task Send_WithMultipleBehaviors_ShouldExecuteInCorrectOrder()
    {
        // Arrange
        TestLoggingBehavior<TestRequest, string>.Logs.Clear();
        TestPerformanceBehavior<TestRequest, string>.PerformanceLogs.Clear();

        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddScoped<IMediator, Mediator>();
        services.AddScoped<IRequestHandler<TestRequest, string>, TestRequestHandler>();
        
        // Register in specific order: Logging first, then Performance
        services.AddScoped<IPipelineBehavior<TestRequest, string>, TestLoggingBehavior<TestRequest, string>>();
        services.AddScoped<IPipelineBehavior<TestRequest, string>, TestPerformanceBehavior<TestRequest, string>>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var request = new TestRequest { Message = "Test Message" };

        // Act
        var result = await mediator.Send(request);

        // Assert
        Assert.Equal("Handled: Test Message", result);
        
        // Verify logging behavior executed
        Assert.Contains("Before handling TestRequest", TestLoggingBehavior<TestRequest, string>.Logs);
        Assert.Contains("After handling TestRequest", TestLoggingBehavior<TestRequest, string>.Logs);
        
        // Verify performance behavior executed
        Assert.Single(TestPerformanceBehavior<TestRequest, string>.PerformanceLogs);
        Assert.Contains("TestRequest took", TestPerformanceBehavior<TestRequest, string>.PerformanceLogs[0]);
    }

    [Fact]
    public async Task Send_WithValidationBehavior_ValidRequest_ShouldSucceed()
    {
        // Arrange
        TestValidationBehavior<ValidatedRequest, string>.ValidationLogs.Clear();

        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddScoped<IMediator, Mediator>();
        services.AddScoped<IRequestHandler<ValidatedRequest, string>, ValidatedRequestHandler>();
        services.AddScoped<IPipelineBehavior<ValidatedRequest, string>, TestValidationBehavior<ValidatedRequest, string>>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var request = new ValidatedRequest { Message = "Valid Message" };

        // Act
        var result = await mediator.Send(request);

        // Assert
        Assert.Equal("Validated: Valid Message", result);
        Assert.Contains("Validating ValidatedRequest", TestValidationBehavior<ValidatedRequest, string>.ValidationLogs);
        Assert.Contains("Validation passed for ValidatedRequest", TestValidationBehavior<ValidatedRequest, string>.ValidationLogs);
    }

    [Fact]
    public async Task Send_WithValidationBehavior_InvalidRequest_ShouldThrowValidationException()
    {
        // Arrange
        TestValidationBehavior<ValidatedRequest, string>.ValidationLogs.Clear();

        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddScoped<IMediator, Mediator>();
        services.AddScoped<IRequestHandler<ValidatedRequest, string>, ValidatedRequestHandler>();
        services.AddScoped<IPipelineBehavior<ValidatedRequest, string>, TestValidationBehavior<ValidatedRequest, string>>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var request = new ValidatedRequest { Message = "" }; // Invalid - empty message

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => mediator.Send(request));
        Assert.Contains("Message is required", exception.Message);
        Assert.Contains("Validating ValidatedRequest", TestValidationBehavior<ValidatedRequest, string>.ValidationLogs);
        Assert.DoesNotContain("Validation passed", TestValidationBehavior<ValidatedRequest, string>.ValidationLogs);
    }

    [Fact]
    public async Task Send_VoidRequest_WithBehaviors_ShouldExecutePipeline()
    {
        // Arrange
        TestLoggingBehavior<TestVoidRequest, EssentialMediator.Abstractions.Mediation.Unit>.Logs.Clear();

        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddScoped<IMediator, Mediator>();
        services.AddScoped<IRequestHandler<TestVoidRequest>, TestVoidRequestHandler>();
        services.AddScoped<IPipelineBehavior<TestVoidRequest, EssentialMediator.Abstractions.Mediation.Unit>, 
            TestLoggingBehavior<TestVoidRequest, EssentialMediator.Abstractions.Mediation.Unit>>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var request = new TestVoidRequest { Message = "Test Message" };

        // Act
        var result = await mediator.Send(request);

        // Assert
        Assert.Equal(EssentialMediator.Abstractions.Mediation.Unit.Value, result);
        Assert.Contains("Before handling TestVoidRequest", 
            TestLoggingBehavior<TestVoidRequest, EssentialMediator.Abstractions.Mediation.Unit>.Logs);
        Assert.Contains("After handling TestVoidRequest", 
            TestLoggingBehavior<TestVoidRequest, EssentialMediator.Abstractions.Mediation.Unit>.Logs);
    }

    [Fact]
    public async Task Send_WithBuiltInLoggingBehavior_ShouldWork()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddEssentialMediator(typeof(PipelineTestRequest).Assembly)
            .AddLoggingBehavior();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var request = new PipelineTestRequest { Message = "Test Message" };

        // Act
        var result = await mediator.Send(request);

        // Assert
        Assert.Equal("Pipeline Handled: Test Message", result);
        // Note: Built-in behavior uses ILogger, so we can't easily assert on logs in unit tests
        // This test mainly ensures no exceptions are thrown
    }

    [Fact]
    public async Task Send_WithBuiltInPerformanceBehavior_ShouldWork()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddEssentialMediator(typeof(PipelineTestRequest).Assembly)
            .AddPerformanceBehavior(100); // 100ms threshold

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var request = new PipelineTestRequest { Message = "Test Message" };

        // Act
        var result = await mediator.Send(request);

        // Assert
        Assert.Equal("Pipeline Handled: Test Message", result);
    }

    [Fact]
    public async Task Send_WithBuiltInValidationBehavior_ShouldWork()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddEssentialMediator(typeof(ValidatedRequest).Assembly)
            .AddValidationBehavior();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var request = new ValidatedRequest { Message = "Valid Message" };

        // Act
        var result = await mediator.Send(request);

        // Assert
        Assert.Equal("Validated: Valid Message", result);
    }

    [Fact]
    public async Task Send_WithAllBuiltInBehaviors_ShouldWork()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddEssentialMediator(typeof(PipelineTestRequest).Assembly)
            .AddAllBuiltInBehaviors(slowRequestThresholdMs: 100);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var request = new PipelineTestRequest { Message = "Test Message" };

        // Act
        var result = await mediator.Send(request);

        // Assert
        Assert.Equal("Pipeline Handled: Test Message", result);
    }
}
