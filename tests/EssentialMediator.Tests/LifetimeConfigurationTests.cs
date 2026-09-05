using EssentialMediator.Abstractions.Handlers;
using EssentialMediator.Extensions;
using EssentialMediator.Extensions.DependencyInjection.Configuration;
using EssentialMediator.Mediation;
using EssentialMediator.Tests.Models.Requests;
using Microsoft.Extensions.DependencyInjection;

namespace EssentialMediator.Tests;

public class LifetimeConfigurationTests
{
    [Fact]
    public void Configuration_DefaultLifetimes_ShouldBeScopedIndependently()
    {
        var configuration = new MediatorConfiguration();

        Assert.Equal(ServiceLifetime.Scoped, configuration.HandlerLifetime);
        Assert.Equal(ServiceLifetime.Scoped, configuration.MediatorLifetime);
    }

    [Fact]
    public void AddEssentialMediator_ShouldAllowIndependentMediatorAndHandlerLifetimes()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddEssentialMediator(configuration =>
        {
            configuration
                .RegisterServicesFromAssemblyContaining<TestRequest>()
                .WithHandlerLifetime(ServiceLifetime.Singleton)
                .WithMediatorLifetime(ServiceLifetime.Transient);
        });

        var mediatorDescriptor = services.Single(descriptor => descriptor.ServiceType == typeof(IMediator));
        var handlerDescriptor = services.Single(descriptor =>
            descriptor.ServiceType == typeof(IRequestHandler<TestRequest, string>)
            && descriptor.ImplementationType == typeof(TestRequestHandler));

        Assert.Equal(ServiceLifetime.Transient, mediatorDescriptor.Lifetime);
        Assert.Equal(ServiceLifetime.Singleton, handlerDescriptor.Lifetime);
    }

    [Fact]
    public void AddEssentialMediator_WithSingletonMediatorLifetime_ShouldThrow()
    {
        var services = new ServiceCollection();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddEssentialMediator(configuration =>
            {
                configuration
                    .RegisterServicesFromAssemblyContaining<TestRequest>()
                    .WithMediatorLifetime(ServiceLifetime.Singleton);
            }));

        Assert.Contains("cannot be registered as Singleton", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void WithHandlerLifetime_ShouldNotChangeMediatorLifetime()
    {
        var configuration = new MediatorConfiguration()
            .WithHandlerLifetime(ServiceLifetime.Singleton);

        Assert.Equal(ServiceLifetime.Singleton, configuration.HandlerLifetime);
        Assert.Equal(ServiceLifetime.Scoped, configuration.MediatorLifetime);
    }

    [Fact]
    public void WithMediatorLifetime_ShouldNotChangeHandlerLifetime()
    {
        var configuration = new MediatorConfiguration()
            .WithMediatorLifetime(ServiceLifetime.Transient);

        Assert.Equal(ServiceLifetime.Scoped, configuration.HandlerLifetime);
        Assert.Equal(ServiceLifetime.Transient, configuration.MediatorLifetime);
    }

#pragma warning disable CS0618
    [Fact]
    public void LegacyWithServiceLifetime_ShouldPreservePreviousSharedLifetimeBehavior()
    {
        var configuration = new MediatorConfiguration()
            .WithServiceLifetime(ServiceLifetime.Singleton);

        Assert.Equal(ServiceLifetime.Singleton, configuration.HandlerLifetime);
        Assert.Equal(ServiceLifetime.Singleton, configuration.MediatorLifetime);
        Assert.Equal(ServiceLifetime.Singleton, configuration.ServiceLifetime);
    }
#pragma warning restore CS0618
}
