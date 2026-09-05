using BenchmarkDotNet.Attributes;
using EssentialMediator.Abstractions.Delegates;
using EssentialMediator.Abstractions.Handlers;
using EssentialMediator.Abstractions.Messages;
using EssentialMediator.Abstractions.Pipelines;
using Microsoft.Extensions.Logging.Abstractions;

namespace EssentialMediator.Benchmarks;

[MemoryDiagnoser]
public class MediatorSendBenchmarks
{
    private readonly PingRequest _request = new(42);
    private PingHandler _handler = null!;
    private Mediator _mediator = null!;
    private Mediator _mediatorWithBehavior = null!;

    [GlobalSetup]
    public void Setup()
    {
        _handler = new PingHandler();
        _mediator = CreateMediator(
            _handler,
            Array.Empty<IPipelineBehavior<PingRequest, int>>());
        _mediatorWithBehavior = CreateMediator(
            _handler,
            new IPipelineBehavior<PingRequest, int>[] { new PassThroughBehavior() });
    }

    [Benchmark(Baseline = true)]
    public Task<int> DirectHandler()
        => _handler.Handle(_request);

    [Benchmark]
    public Task<int> MediatorSend()
        => _mediator.Send(_request);

    [Benchmark]
    public Task<int> MediatorSendWithOneBehavior()
        => _mediatorWithBehavior.Send(_request);

    private static Mediator CreateMediator(
        IRequestHandler<PingRequest, int> handler,
        IEnumerable<IPipelineBehavior<PingRequest, int>> behaviors)
    {
        var provider = new DictionaryServiceProvider();
        provider.Register<IEnumerable<IRequestHandler<PingRequest, int>>>(
            new[] { handler });
        provider.Register<IEnumerable<IPipelineBehavior<PingRequest, int>>>(
            behaviors.ToArray());

        return new Mediator(provider, NullLogger<Mediator>.Instance);
    }
}

[MemoryDiagnoser]
public class MediatorPublishBenchmarks
{
    private readonly PingNotification _notification = new();
    private PingNotificationHandler _handler = null!;
    private Mediator _mediator = null!;

    [GlobalSetup]
    public void Setup()
    {
        _handler = new PingNotificationHandler();
        var provider = new DictionaryServiceProvider();
        provider.Register<IEnumerable<INotificationHandler<PingNotification>>>(
            new INotificationHandler<PingNotification>[] { _handler });
        _mediator = new Mediator(provider, NullLogger<Mediator>.Instance);
    }

    [Benchmark(Baseline = true)]
    public Task DirectHandler()
        => _handler.Handle(_notification);

    [Benchmark]
    public Task MediatorPublish()
        => _mediator.Publish(_notification);
}

public sealed record PingRequest(int Value) : IRequest<int>;

public sealed class PingHandler : IRequestHandler<PingRequest, int>
{
    public Task<int> Handle(PingRequest request, CancellationToken cancellationToken = default)
        => Task.FromResult(request.Value);
}

public sealed class PassThroughBehavior : IPipelineBehavior<PingRequest, int>
{
    public Task<int> Handle(
        PingRequest request,
        RequestHandlerDelegate<int> next,
        CancellationToken cancellationToken)
        => next();
}

public sealed class PingNotification : INotification;

public sealed class PingNotificationHandler : INotificationHandler<PingNotification>
{
    public Task Handle(PingNotification notification, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}

internal sealed class DictionaryServiceProvider : IServiceProvider
{
    private readonly Dictionary<Type, object> _services = new();

    internal void Register<TService>(TService service)
        where TService : notnull
        => _services[typeof(TService)] = service;

    public object? GetService(Type serviceType)
        => _services.TryGetValue(serviceType, out var service) ? service : null;
}
