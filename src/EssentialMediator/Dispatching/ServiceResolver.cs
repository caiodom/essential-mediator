namespace EssentialMediator.Dispatching;

internal static class ServiceResolver
{
    internal static IEnumerable<TService> GetServices<TService>(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        return serviceProvider.GetService(typeof(IEnumerable<TService>)) as IEnumerable<TService>
            ?? Array.Empty<TService>();
    }
}
