using EssentialMediator.Abstractions.Delegates;
using EssentialMediator.Abstractions.Handlers;
using EssentialMediator.Abstractions.Messages;
using EssentialMediator.Abstractions.Pipelines;
using EssentialMediator.Mediation;
using Microsoft.Extensions.DependencyInjection;

namespace EssentialMediator.Tests;

public class TypedDispatchTests
{
    [Fact]
    public async Task Send_WithExplicitInterfaceHandler_ShouldDispatchWithoutMethodReflection()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped<IMediator, Mediator>();
        services.AddScoped<IRequestHandler<ExplicitRequest, string>, ExplicitRequestHandler>();

        using var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var result = await mediator.Send(new ExplicitRequest("value"));

        Assert.Equal("explicit:value", result);
    }

    [Fact]
    public async Task Publish_WithExplicitInterfaceHandler_ShouldDispatchWithoutMethodReflection()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped<IMediator, Mediator>();
        services.AddScoped<ExplicitNotificationHandler>();
        services.AddScoped<INotificationHandler<ExplicitNotification>>(sp =>
            sp.GetRequiredService<ExplicitNotificationHandler>());

        using var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var handler = serviceProvider.GetRequiredService<ExplicitNotificationHandler>();

        await mediator.Publish(new ExplicitNotification());

        Assert.True(handler.WasCalled);
    }

    [Fact]
    public async Task Send_WithExplicitInterfacePipelineBehavior_ShouldDispatchWithoutMethodReflection()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped<IMediator, Mediator>();
        services.AddScoped<IRequestHandler<ExplicitBehaviorRequest, string>, ExplicitBehaviorRequestHandler>();
        services.AddScoped<ExplicitPipelineBehavior>();
        services.AddScoped<IPipelineBehavior<ExplicitBehaviorRequest, string>>(sp =>
            sp.GetRequiredService<ExplicitPipelineBehavior>());

        using var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var behavior = serviceProvider.GetRequiredService<ExplicitPipelineBehavior>();

        var result = await mediator.Send(new ExplicitBehaviorRequest());

        Assert.Equal("handled", result);
        Assert.True(behavior.WasCalled);
    }

    private sealed record ExplicitRequest(string Value) : IRequest<string>;

    private sealed class ExplicitRequestHandler : IRequestHandler<ExplicitRequest, string>
    {
        Task<string> IRequestHandler<ExplicitRequest, string>.Handle(
            ExplicitRequest request,
            CancellationToken cancellationToken)
            => Task.FromResult($"explicit:{request.Value}");
    }

    private sealed class ExplicitNotification : INotification;

    private sealed class ExplicitNotificationHandler : INotificationHandler<ExplicitNotification>
    {
        public bool WasCalled { get; private set; }

        Task INotificationHandler<ExplicitNotification>.Handle(
            ExplicitNotification notification,
            CancellationToken cancellationToken)
        {
            WasCalled = true;
            return Task.CompletedTask;
        }
    }

    private sealed class ExplicitBehaviorRequest : IRequest<string>;

    private sealed class ExplicitBehaviorRequestHandler : IRequestHandler<ExplicitBehaviorRequest, string>
    {
        public Task<string> Handle(
            ExplicitBehaviorRequest request,
            CancellationToken cancellationToken = default)
            => Task.FromResult("handled");
    }

    private sealed class ExplicitPipelineBehavior : IPipelineBehavior<ExplicitBehaviorRequest, string>
    {
        public bool WasCalled { get; private set; }

        Task<string> IPipelineBehavior<ExplicitBehaviorRequest, string>.Handle(
            ExplicitBehaviorRequest request,
            RequestHandlerDelegate<string> next,
            CancellationToken cancellationToken)
        {
            WasCalled = true;
            return next();
        }
    }
}
