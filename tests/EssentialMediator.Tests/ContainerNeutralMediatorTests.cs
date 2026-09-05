using EssentialMediator.Abstractions.Handlers;
using EssentialMediator.Abstractions.Messages;
using EssentialMediator.Abstractions.Pipelines;
using Microsoft.Extensions.Logging.Abstractions;

namespace EssentialMediator.Tests;

public class ContainerNeutralMediatorTests
{
    [Fact]
    public async Task Send_ShouldWorkWithPlainIServiceProviderImplementation()
    {
        var provider = new DictionaryServiceProvider();
        provider.Register<IEnumerable<IRequestHandler<StandaloneRequest, string>>>(
            new IRequestHandler<StandaloneRequest, string>[] { new StandaloneRequestHandler() });
        provider.Register<IEnumerable<IPipelineBehavior<StandaloneRequest, string>>>(
            Array.Empty<IPipelineBehavior<StandaloneRequest, string>>());

        var mediator = new Mediator(provider, NullLogger<Mediator>.Instance);

        var result = await mediator.Send(new StandaloneRequest("hello"));

        Assert.Equal("handled:hello", result);
    }

    [Fact]
    public async Task Publish_ShouldWorkWithPlainIServiceProviderImplementation()
    {
        var handler = new StandaloneNotificationHandler();
        var provider = new DictionaryServiceProvider();
        provider.Register<IEnumerable<INotificationHandler<StandaloneNotification>>>(
            new INotificationHandler<StandaloneNotification>[] { handler });

        var mediator = new Mediator(provider, NullLogger<Mediator>.Instance);

        await mediator.Publish(new StandaloneNotification());

        Assert.True(handler.WasCalled);
    }

    private sealed class DictionaryServiceProvider : IServiceProvider
    {
        private readonly Dictionary<Type, object> _services = new();

        public void Register<TService>(TService service) where TService : notnull
            => _services[typeof(TService)] = service;

        public object? GetService(Type serviceType)
            => _services.TryGetValue(serviceType, out var service) ? service : null;
    }

    private sealed record StandaloneRequest(string Value) : IRequest<string>;

    private sealed class StandaloneRequestHandler : IRequestHandler<StandaloneRequest, string>
    {
        public Task<string> Handle(StandaloneRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult($"handled:{request.Value}");
    }

    private sealed class StandaloneNotification : INotification;

    private sealed class StandaloneNotificationHandler : INotificationHandler<StandaloneNotification>
    {
        public bool WasCalled { get; private set; }

        public Task Handle(StandaloneNotification notification, CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            return Task.CompletedTask;
        }
    }
}
