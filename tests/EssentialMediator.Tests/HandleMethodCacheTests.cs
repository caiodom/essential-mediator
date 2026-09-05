using EssentialMediator.Abstractions.Delegates;
using EssentialMediator.Abstractions.Handlers;
using EssentialMediator.Abstractions.Messages;
using EssentialMediator.Abstractions.Pipelines;
using EssentialMediator.Mediation;
using Microsoft.Extensions.DependencyInjection;

namespace EssentialMediator.Tests;

public class HandleMethodCacheTests
{
    [Fact]
    public async Task Send_WhenOneConcreteHandlerHandlesMultipleRequestTypes_ShouldUseCorrectHandleMethod()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped<IMediator, Mediator>();
        services.AddScoped<MultiRequestHandler>();
        services.AddScoped<IRequestHandler<FirstCacheRequest, string>>(sp => sp.GetRequiredService<MultiRequestHandler>());
        services.AddScoped<IRequestHandler<SecondCacheRequest, int>>(sp => sp.GetRequiredService<MultiRequestHandler>());

        using var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var firstResult = await mediator.Send(new FirstCacheRequest("first"));
        var secondResult = await mediator.Send(new SecondCacheRequest(42));

        Assert.Equal("handled:first", firstResult);
        Assert.Equal(84, secondResult);
    }

    [Fact]
    public async Task Publish_WhenOneConcreteHandlerHandlesMultipleNotificationTypes_ShouldUseCorrectHandleMethod()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped<IMediator, Mediator>();
        services.AddScoped<MultiNotificationHandler>();
        services.AddScoped<INotificationHandler<FirstCacheNotification>>(sp => sp.GetRequiredService<MultiNotificationHandler>());
        services.AddScoped<INotificationHandler<SecondCacheNotification>>(sp => sp.GetRequiredService<MultiNotificationHandler>());

        using var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var handler = serviceProvider.GetRequiredService<MultiNotificationHandler>();

        await mediator.Publish(new FirstCacheNotification());
        await mediator.Publish(new SecondCacheNotification());

        Assert.Equal(1, handler.FirstHandledCount);
        Assert.Equal(1, handler.SecondHandledCount);
    }

    [Fact]
    public async Task Send_WhenOneConcreteBehaviorHandlesMultipleRequestTypes_ShouldUseCorrectHandleMethod()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped<IMediator, Mediator>();
        services.AddScoped<IRequestHandler<FirstBehaviorRequest, string>, FirstBehaviorRequestHandler>();
        services.AddScoped<IRequestHandler<SecondBehaviorRequest, int>, SecondBehaviorRequestHandler>();
        services.AddScoped<MultiPipelineBehavior>();
        services.AddScoped<IPipelineBehavior<FirstBehaviorRequest, string>>(sp => sp.GetRequiredService<MultiPipelineBehavior>());
        services.AddScoped<IPipelineBehavior<SecondBehaviorRequest, int>>(sp => sp.GetRequiredService<MultiPipelineBehavior>());

        using var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var behavior = serviceProvider.GetRequiredService<MultiPipelineBehavior>();

        var firstResult = await mediator.Send(new FirstBehaviorRequest());
        var secondResult = await mediator.Send(new SecondBehaviorRequest());

        Assert.Equal("first", firstResult);
        Assert.Equal(2, secondResult);
        Assert.Equal(1, behavior.FirstHandledCount);
        Assert.Equal(1, behavior.SecondHandledCount);
    }

    private sealed record FirstCacheRequest(string Value) : IRequest<string>;
    private sealed record SecondCacheRequest(int Value) : IRequest<int>;

    private sealed class MultiRequestHandler :
        IRequestHandler<FirstCacheRequest, string>,
        IRequestHandler<SecondCacheRequest, int>
    {
        public Task<string> Handle(FirstCacheRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult($"handled:{request.Value}");

        public Task<int> Handle(SecondCacheRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(request.Value * 2);
    }

    private sealed class FirstCacheNotification : INotification;
    private sealed class SecondCacheNotification : INotification;

    private sealed class MultiNotificationHandler :
        INotificationHandler<FirstCacheNotification>,
        INotificationHandler<SecondCacheNotification>
    {
        public int FirstHandledCount { get; private set; }
        public int SecondHandledCount { get; private set; }

        public Task Handle(FirstCacheNotification notification, CancellationToken cancellationToken = default)
        {
            FirstHandledCount++;
            return Task.CompletedTask;
        }

        public Task Handle(SecondCacheNotification notification, CancellationToken cancellationToken = default)
        {
            SecondHandledCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class FirstBehaviorRequest : IRequest<string>;
    private sealed class SecondBehaviorRequest : IRequest<int>;

    private sealed class FirstBehaviorRequestHandler : IRequestHandler<FirstBehaviorRequest, string>
    {
        public Task<string> Handle(FirstBehaviorRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult("first");
    }

    private sealed class SecondBehaviorRequestHandler : IRequestHandler<SecondBehaviorRequest, int>
    {
        public Task<int> Handle(SecondBehaviorRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(2);
    }

    private sealed class MultiPipelineBehavior :
        IPipelineBehavior<FirstBehaviorRequest, string>,
        IPipelineBehavior<SecondBehaviorRequest, int>
    {
        public int FirstHandledCount { get; private set; }
        public int SecondHandledCount { get; private set; }

        public async Task<string> Handle(
            FirstBehaviorRequest request,
            RequestHandlerDelegate<string> next,
            CancellationToken cancellationToken)
        {
            FirstHandledCount++;
            return await next();
        }

        public async Task<int> Handle(
            SecondBehaviorRequest request,
            RequestHandlerDelegate<int> next,
            CancellationToken cancellationToken)
        {
            SecondHandledCount++;
            return await next();
        }
    }
}
