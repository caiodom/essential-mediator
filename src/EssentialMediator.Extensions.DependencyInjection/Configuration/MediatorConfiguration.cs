using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace EssentialMediator.Extensions.DependencyInjection.Configuration;

/// <summary>
/// Configuration for EssentialMediator
/// </summary>
public class MediatorConfiguration
{
    internal HashSet<Assembly> Assemblies { get; } = new();

    /// <summary>
    /// Gets or sets the service lifetime for handlers. Default is Scoped.
    /// </summary>
    public ServiceLifetime ServiceLifetime { get; set; } = ServiceLifetime.Scoped;

    /// <summary>
    /// Register handlers from specified assemblies
    /// </summary>
    /// <param name="assemblies">Assemblies to scan</param>
    /// <returns>The configuration instance for method chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when assemblies is null</exception>
    public MediatorConfiguration RegisterServicesFromAssemblies(params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);

        foreach (var assembly in assemblies.Where(a => a != null))
        {
            Assemblies.Add(assembly);
        }
        return this;
    }

    /// <summary>
    /// Register handlers from assembly containing the specified type
    /// </summary>
    /// <typeparam name="T">Type to locate assembly</typeparam>
    /// <returns>The configuration instance for method chaining</returns>
    public MediatorConfiguration RegisterServicesFromAssemblyContaining<T>()
    {
        Assemblies.Add(typeof(T).Assembly);
        return this;
    }

    /// <summary>
    /// Register handlers from assembly containing the specified type
    /// </summary>
    /// <param name="type">Type to locate assembly</param>
    /// <returns>The configuration instance for method chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when type is null</exception>
    public MediatorConfiguration RegisterServicesFromAssemblyContaining(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        Assemblies.Add(type.Assembly);
        return this;
    }

    /// <summary>
    /// Sets the service lifetime for handlers
    /// </summary>
    /// <param name="lifetime">The service lifetime to use</param>
    /// <returns>The configuration instance for method chaining</returns>
    public MediatorConfiguration WithServiceLifetime(ServiceLifetime lifetime)
    {
        ServiceLifetime = lifetime;
        return this;
    }

    /// <summary>
    /// Register handlers from the entry assembly
    /// </summary>
    /// <returns>The configuration instance for method chaining</returns>
    public MediatorConfiguration RegisterServicesFromEntryAssembly()
    {
        var entryAssembly = Assembly.GetEntryAssembly();
        if (entryAssembly != null)
            Assemblies.Add(entryAssembly);

        return this;
    }

    /// <summary>
    /// Register handlers from the calling assembly
    /// </summary>
    /// <returns>The configuration instance for method chaining</returns>
    public MediatorConfiguration RegisterServicesFromCallingAssembly()
    {
        Assemblies.Add(Assembly.GetCallingAssembly());
        return this;
    }
}
