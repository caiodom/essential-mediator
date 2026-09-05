using EssentialMediator.Abstractions.Pipelines;
using EssentialMediator.Behaviors;
using EssentialMediator.Extensions;
using EssentialMediator.Tests.Models.Requests;
using Microsoft.Extensions.DependencyInjection;

namespace EssentialMediator.Tests;

public class PerformanceBehaviorConfigurationTests
{
    [Fact]
    public void AddPerformanceBehavior_ShouldRegisterConfiguredThreshold()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddEssentialMediator(typeof(TestRequest).Assembly)
            .AddPerformanceBehavior(slowRequestThresholdMs: 1234);

        using var serviceProvider = services.BuildServiceProvider();

        var options = serviceProvider.GetRequiredService<PerformanceBehaviorOptions>();
        var behaviors = serviceProvider.GetServices<IPipelineBehavior<TestRequest, string>>();

        Assert.Equal(1234, options.SlowRequestThresholdMs);
        Assert.Contains(behaviors, behavior => behavior is PerformanceBehavior<TestRequest, string>);
    }

    [Fact]
    public void AddPerformanceBehavior_WithNegativeThreshold_ShouldThrow()
    {
        var services = new ServiceCollection();

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            services.AddPerformanceBehavior(slowRequestThresholdMs: -1));

        Assert.Equal("slowRequestThresholdMs", exception.ParamName);
    }
}
