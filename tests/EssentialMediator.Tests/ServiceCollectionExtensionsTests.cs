using EssentialMediator.Abstractions.Handlers;
using EssentialMediator.Abstractions.Pipelines;
using EssentialMediator.Extensions;
using EssentialMediator.Mediation;
using EssentialMediator.Tests.Models.Behaviors;
using EssentialMediator.Tests.Models.Requests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EssentialMediator.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddEssentialMediator_WithAssembly_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddEssentialMediator(typeof(TestRequest).Assembly);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var mediator = serviceProvider.GetService<IMediator>();
        Assert.NotNull(mediator);
        Assert.IsType<Mediator>(mediator);
    }

    [Fact]
    public void AddEssentialMediator_WithConfiguration_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddEssentialMediator(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<TestRequest>();
            cfg.WithServiceLifetime(ServiceLifetime.Singleton);
        });
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var mediator1 = serviceProvider.GetService<IMediator>();
        var mediator2 = serviceProvider.GetService<IMediator>();
        Assert.NotNull(mediator1);
        Assert.Same(mediator1, mediator2); // Should be singleton
    }

    [Fact]
    public void AddPipelineBehavior_Generic_ShouldRegisterBehavior()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddEssentialMediator(typeof(TestRequest).Assembly);

        // Act
        services.AddPipelineBehavior<TestLoggingBehavior<TestRequest, string>>();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var behavior = serviceProvider.GetService<IPipelineBehavior<TestRequest, string>>();
        Assert.NotNull(behavior);
        Assert.IsType<TestLoggingBehavior<TestRequest, string>>(behavior);
    }

    [Fact]
    public void AddPipelineBehavior_Type_ShouldRegisterBehavior()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddEssentialMediator(typeof(TestRequest).Assembly);

        // Act
        services.AddPipelineBehavior(typeof(TestLoggingBehavior<TestRequest, string>));
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var behavior = serviceProvider.GetService<IPipelineBehavior<TestRequest, string>>();
        Assert.NotNull(behavior);
        Assert.IsType<TestLoggingBehavior<TestRequest, string>>(behavior);
    }

    [Fact]
    public void AddPipelineBehavior_InvalidType_ShouldThrowArgumentException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            services.AddPipelineBehavior(typeof(string))); // string doesn't implement IPipelineBehavior

        Assert.Contains("does not implement IPipelineBehavior", exception.Message);
    }

    [Fact]
    public void AddLoggingBehavior_ShouldRegisterBuiltInBehavior()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddEssentialMediator(typeof(TestRequest).Assembly);

        // Act
        services.AddLoggingBehavior();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var behaviors = serviceProvider.GetServices<IPipelineBehavior<TestRequest, string>>();
        Assert.NotEmpty(behaviors);
    }

    [Fact]
    public void AddPerformanceBehavior_ShouldRegisterBuiltInBehavior()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddEssentialMediator(typeof(TestRequest).Assembly);

        // Act
        services.AddPerformanceBehavior(1000);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var behaviors = serviceProvider.GetServices<IPipelineBehavior<TestRequest, string>>();
        Assert.NotEmpty(behaviors);
    }

    [Fact]
    public void AddValidationBehavior_ShouldRegisterBuiltInBehavior()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddEssentialMediator(typeof(ValidatedRequest).Assembly);

        // Act
        services.AddValidationBehavior();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var behaviors = serviceProvider.GetServices<IPipelineBehavior<ValidatedRequest, string>>();
        Assert.NotEmpty(behaviors);
    }

    [Fact]
    public void AddAllBuiltInBehaviors_ShouldRegisterAllBehaviors()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddEssentialMediator(typeof(TestRequest).Assembly);

        // Act
        services.AddAllBuiltInBehaviors(500);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var behaviors = serviceProvider.GetServices<IPipelineBehavior<TestRequest, string>>();
        Assert.NotEmpty(behaviors);
        
        // Should have multiple behaviors registered
        Assert.True(behaviors.Count() >= 3); // Logging, Performance, Validation
    }

    [Fact]
    public void AddEssentialMediator_ChainableCalls_ShouldWork()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        var result = services
            .AddEssentialMediator(typeof(TestRequest).Assembly)
            .AddLoggingBehavior()
            .AddPerformanceBehavior(200)
            .AddValidationBehavior();

        // Assert
        Assert.Same(services, result); // Should return the same IServiceCollection for chaining
        
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetService<IMediator>();
        Assert.NotNull(mediator);
    }

    [Fact]
    public void AddEssentialMediator_WithDifferentLifetimes_ShouldRespectLifetime()
    {
        // Arrange & Act
        var services1 = new ServiceCollection();
        services1.AddLogging();
        services1.AddEssentialMediator(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<TestRequest>();
            cfg.WithServiceLifetime(ServiceLifetime.Singleton);
        });

        var services2 = new ServiceCollection();
        services2.AddLogging();
        services2.AddEssentialMediator(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<TestRequest>();
            cfg.WithServiceLifetime(ServiceLifetime.Transient);
        });

        // Assert
        var sp1 = services1.BuildServiceProvider();
        var mediator1a = sp1.GetService<IMediator>();
        var mediator1b = sp1.GetService<IMediator>();
        Assert.Same(mediator1a, mediator1b); // Singleton

        var sp2 = services2.BuildServiceProvider();
        var mediator2a = sp2.GetService<IMediator>();
        var mediator2b = sp2.GetService<IMediator>();
        Assert.NotSame(mediator2a, mediator2b); // Transient
    }
}
