using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace EssentialMediator.Extensions.DependencyInjection.Configuration;

/// <summary>
/// Configuration for EssentialMediator.
/// </summary>
public class MediatorConfiguration
{
    internal HashSet<Assembly> Assemblies { get; } = new();

    /// <summary>
    /// Gets or sets the service lifetime for request and notification handlers. Default is Scoped.
    /// </summary>
    public ServiceLifetime HandlerLifetime { get; set; } = ServiceLifetime.Scoped;

    /// <summary>
    /// Gets or sets the service lifetime for <see cref="EssentialMediator.Mediation.IMediator"/>.
    /// Default is Scoped. Scoped and Transient are supported by the Microsoft DI integration;
    /// Singleton is rejected because the mediator resolves handlers and pipeline behaviors from the ambient scope.
    /// </summary>
    public ServiceLifetime MediatorLifetime { get; set; } = ServiceLifetime.Scoped;

    /// <summary>
    /// Gets or sets the legacy shared lifetime. Setting this property configures both handler and mediator lifetimes.
    /// </summary>
    [Obsolete("Use HandlerLifetime and MediatorLifetime to configure lifetimes independently.")]
    public ServiceLifetime ServiceLifetime
    {
        get => HandlerLifetime;
        set
        {
            HandlerLifetime = value;
            MediatorLifetime = value;
        }
    }

    /// <summary>
    /// Register handlers from specified assemblies.
    /// </summary>
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
    /// Register handlers from assembly containing the specified type.
    /// </summary>
    public MediatorConfiguration RegisterServicesFromAssemblyContaining<T>()
    {
        Assemblies.Add(typeof(T).Assembly);
        return this;
    }

    /// <summary>
    /// Register handlers from assembly containing the specified type.
    /// </summary>
    public MediatorConfiguration RegisterServicesFromAssemblyContaining(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        Assemblies.Add(type.Assembly);
        return this;
    }

    /// <summary>
    /// Sets the handler lifetime without changing the mediator lifetime.
    /// </summary>
    public MediatorConfiguration WithHandlerLifetime(ServiceLifetime lifetime)
    {
        HandlerLifetime = lifetime;
        return this;
    }

    /// <summary>
    /// Sets the mediator lifetime without changing handler lifetimes.
    /// Scoped is recommended and Transient is supported. Singleton is rejected during service registration.
    /// </summary>
    public MediatorConfiguration WithMediatorLifetime(ServiceLifetime lifetime)
    {
        MediatorLifetime = lifetime;
        return this;
    }

    /// <summary>
    /// Sets both mediator and handler lifetimes for backward compatibility.
    /// Singleton mediator lifetime is rejected during service registration.
    /// </summary>
    [Obsolete("Use WithHandlerLifetime and WithMediatorLifetime to configure lifetimes independently.")]
    public MediatorConfiguration WithServiceLifetime(ServiceLifetime lifetime)
    {
        HandlerLifetime = lifetime;
        MediatorLifetime = lifetime;
        return this;
    }

    /// <summary>
    /// Register handlers from the entry assembly.
    /// </summary>
    public MediatorConfiguration RegisterServicesFromEntryAssembly()
    {
        var entryAssembly = Assembly.GetEntryAssembly();
        if (entryAssembly != null)
        {
            Assemblies.Add(entryAssembly);
        }

        return this;
    }

    /// <summary>
    /// Register handlers from the calling assembly.
    /// </summary>
    public MediatorConfiguration RegisterServicesFromCallingAssembly()
    {
        Assemblies.Add(Assembly.GetCallingAssembly());
        return this;
    }
}
