using EssentialMediator.Abstractions.Handlers;
using EssentialMediator.Abstractions.Messages;
using EssentialMediator.Abstractions.Mediation;
using EssentialMediator.Mediation;
using EssentialMediator.Tests.Models.Requests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EssentialMediator.Tests;

public class ReflectionEdgeCaseTests
{
    [Fact]
    public async Task Send_WithCancellationTokenRequested_ShouldCancelGracefully()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddScoped<IMediator, Mediator>();
        services.AddScoped<IRequestHandler<CancellationTestRequest, string>, CancellationTestRequestHandler>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        var request = new CancellationTestRequest { Message = "Test" };

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => mediator.Send(request, cts.Token));
    }

    [Fact]
    public async Task Send_WithVoidRequestAndCancellation_ShouldCancelGracefully()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddScoped<IMediator, Mediator>();
        services.AddScoped<IRequestHandler<CancellationTestVoidRequest>, CancellationTestVoidRequestHandler>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        var request = new CancellationTestVoidRequest { Message = "Test" };

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => mediator.Send(request, cts.Token));
    }

    [Fact]
    public async Task Send_WithHandlerThrowingOtherException_ShouldWrapCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddScoped<IMediator, Mediator>();
        services.AddScoped<IRequestHandler<ErrorTestRequest, string>, ErrorTestRequestHandler>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var request = new ErrorTestRequest { Message = "Test", ShouldThrow = true };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => mediator.Send(request));
        Assert.Equal("Handler error", exception.Message);
    }

    [Fact]
    public async Task Send_WithVoidHandlerThrowingException_ShouldWrapCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddScoped<IMediator, Mediator>();
        services.AddScoped<IRequestHandler<ErrorTestVoidRequest>, ErrorTestVoidRequestHandler>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var request = new ErrorTestVoidRequest { Message = "Test", ShouldThrow = true };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => mediator.Send(request));
        Assert.Equal("Handler error", exception.Message);
    }
}

// Test models for reflection edge cases
public class CancellationTestRequest : IRequest<string>
{
    public string Message { get; set; } = string.Empty;
}

public class CancellationTestRequestHandler : IRequestHandler<CancellationTestRequest, string>
{
    public Task<string> Handle(CancellationTestRequest request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult($"Handled: {request.Message}");
    }
}

public class CancellationTestVoidRequest : IRequest
{
    public string Message { get; set; } = string.Empty;
}

public class CancellationTestVoidRequestHandler : IRequestHandler<CancellationTestVoidRequest>
{
    public Task<Unit> Handle(CancellationTestVoidRequest request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Unit.Value);
    }
}

public class ErrorTestRequest : IRequest<string>
{
    public string Message { get; set; } = string.Empty;
    public bool ShouldThrow { get; set; }
}

public class ErrorTestRequestHandler : IRequestHandler<ErrorTestRequest, string>
{
    public Task<string> Handle(ErrorTestRequest request, CancellationToken cancellationToken = default)
    {
        if (request.ShouldThrow)
        {
            throw new InvalidOperationException("Handler error");
        }
        return Task.FromResult($"Handled: {request.Message}");
    }
}

public class ErrorTestVoidRequest : IRequest
{
    public string Message { get; set; } = string.Empty;
    public bool ShouldThrow { get; set; }
}

public class ErrorTestVoidRequestHandler : IRequestHandler<ErrorTestVoidRequest>
{
    public Task<Unit> Handle(ErrorTestVoidRequest request, CancellationToken cancellationToken = default)
    {
        if (request.ShouldThrow)
        {
            throw new InvalidOperationException("Handler error");
        }
        return Task.FromResult(Unit.Value);
    }
}
