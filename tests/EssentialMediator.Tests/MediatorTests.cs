using EssentialMediator.Abstractions.Handlers;
using EssentialMediator.Abstractions.Mediation;
using EssentialMediator.Exceptions;
using EssentialMediator.Mediation;
using EssentialMediator.Tests.Models.Requests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EssentialMediator.Tests;

public class DuplicateTestRequestHandler : IRequestHandler<TestRequest, string>
{
    public Task<string> Handle(TestRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult($"Duplicate Handler: {request.Message}");
    }
}

public class MediatorTests
{
    private IServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddScoped<IMediator, Mediator>();

        services.AddScoped<IRequestHandler<TestRequest, string>, TestRequestHandler>();
        services.AddScoped<IRequestHandler<TestVoidRequest>, TestVoidRequestHandler>();
        services.AddScoped<IRequestHandler<AnotherTestRequest, string>, AnotherTestRequestHandler>();

        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task Send_WithValidRequest_ShouldReturnExpectedResponse()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var request = new TestRequest { Message = "Test Message" };

        // Act
        var result = await mediator.Send(request);

        // Assert
        Assert.Equal("Handled: Test Message", result);
    }

    [Fact]
    public async Task Send_WithVoidRequest_ShouldReturnUnit()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var request = new TestVoidRequest { Message = "Test Message" };

        // Act
        var result = await mediator.Send(request);

        // Assert
        Assert.Equal(Unit.Value, result);
    }

    [Fact]
    public async Task Send_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => mediator.Send<string>(null!));
    }

    [Fact]
    public async Task Send_WithUnregisteredHandler_ShouldThrowHandlerNotFoundException()
    {
        // Arrange 
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddScoped<IMediator, Mediator>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var request = new UnregisteredRequest { Message = "Test Message" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HandlerNotFoundException>(() => mediator.Send(request));
        Assert.Equal(typeof(UnregisteredRequest), exception.RequestType);
        Assert.Contains("UnregisteredRequest", exception.Message);
    }

    [Fact]
    public async Task Send_WithMultipleHandlers_ShouldThrowMultipleHandlersException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddScoped<IMediator, Mediator>();

      
        services.AddScoped<IRequestHandler<TestRequest, string>, TestRequestHandler>();
        services.AddScoped<IRequestHandler<TestRequest, string>, DuplicateTestRequestHandler>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var request = new TestRequest { Message = "Test Message" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<MultipleHandlersException>(() => mediator.Send(request));
        Assert.Equal(typeof(TestRequest), exception.RequestType);
        Assert.Equal(2, exception.HandlerCount);
        Assert.Contains("Multiple handlers (2) found", exception.Message);
    }
}


