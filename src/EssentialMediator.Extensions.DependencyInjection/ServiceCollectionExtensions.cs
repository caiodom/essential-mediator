using System.Reflection;
using EssentialMediator.Abstractions.Handlers;
using EssentialMediator.Extensions.DependencyInjection.Configuration;
using EssentialMediator.Mediation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EssentialMediator.Extensions;

/// <summary>
/// Extensions for configuring EssentialMediator in DI container
/// </summary>
public static class ServiceCollectionExtensions
{
    private static readonly Type[] RequestHandlerTypes =
    {
        typeof(IRequestHandler<,>),
        typeof(IRequestHandler<>)
    };

    private static readonly Type NotificationHandlerType = typeof(INotificationHandler<>);

    /// <summary>
    /// Adds EssentialMediator services to the specified IServiceCollection
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to</param>
    /// <param name="assemblies">Assemblies to scan for handlers</param>
    /// <returns>The IServiceCollection so that additional calls can be chained</returns>
    /// <exception cref="ArgumentNullException">Thrown when services is null</exception>
    public static IServiceCollection AddEssentialMediator(this IServiceCollection services, params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        assemblies ??= Array.Empty<Assembly>();

        if (assemblies.Length == 0)
        {
            assemblies = new[] { Assembly.GetCallingAssembly() };
        }

        return AddEssentialMediatorCore(services, assemblies, ServiceLifetime.Scoped);
    }

    /// <summary>
    /// Adds EssentialMediator services to the specified IServiceCollection
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to</param>
    /// <param name="configuration">Configuration action</param>
    /// <returns>The IServiceCollection so that additional calls can be chained</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or configuration is null</exception>
    public static IServiceCollection AddEssentialMediator(this IServiceCollection services,
        Action<MediatorConfiguration> configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var config = new MediatorConfiguration();
        configuration(config);

        return AddEssentialMediatorCore(services, config.Assemblies.ToArray(), config.ServiceLifetime);
    }

    /// <summary>
    /// Core method for adding EssentialMediator services
    /// </summary>
    private static IServiceCollection AddEssentialMediatorCore(IServiceCollection services, Assembly[] assemblies, ServiceLifetime serviceLifetime)
    {
        // Register the mediator with specified lifetime
        switch (serviceLifetime)
        {
            case ServiceLifetime.Singleton:
                services.AddSingleton<IMediator, Mediator>();
                break;
            case ServiceLifetime.Transient:
                services.AddTransient<IMediator, Mediator>();
                break;
            case ServiceLifetime.Scoped:
            default:
                services.AddScoped<IMediator, Mediator>();
                break;
        }

        // Use HashSet to avoid duplicate assemblies
        var uniqueAssemblies = new HashSet<Assembly>(assemblies);

        foreach (var assembly in uniqueAssemblies)
        {
            RegisterHandlers(services, assembly, serviceLifetime);
        }

        return services;
    }

    private static void RegisterHandlers(IServiceCollection services, Assembly assembly, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        try
        {
            var handlerTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.IsPublic)
                .ToList();

            // Register request handlers
            RegisterRequestHandlers(services, handlerTypes, serviceLifetime);

            // Register notification handlers  
            RegisterNotificationHandlers(services, handlerTypes, serviceLifetime);
        }
        catch (ReflectionTypeLoadException ex)
        {
            // Handle assembly loading issues gracefully
            var loadableTypes = ex.Types.Where(t => t != null).Cast<Type>().ToList();
            if (loadableTypes.Any())
            {
                RegisterRequestHandlers(services, loadableTypes, serviceLifetime);
                RegisterNotificationHandlers(services, loadableTypes, serviceLifetime);
            }
        }
    }

    private static void RegisterRequestHandlers(IServiceCollection services, IEnumerable<Type> types, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        foreach (var type in types)
        {
            try
            {
                var interfaces = type.GetInterfaces()
                    .Where(i => i.IsGenericType && RequestHandlerTypes.Contains(i.GetGenericTypeDefinition()))
                    .ToList();

                foreach (var @interface in interfaces)
                {
                    RegisterService(services, @interface, type, serviceLifetime);
                }
            }
            catch (Exception)
            {
                // Skip types that can't be processed
                continue;
            }
        }
    }

    private static void RegisterNotificationHandlers(IServiceCollection services, IEnumerable<Type> types, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        foreach (var type in types)
        {
            try
            {
                var interfaces = type.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == NotificationHandlerType)
                    .ToList();

                foreach (var @interface in interfaces)
                {
                    RegisterService(services, @interface, type, serviceLifetime);
                }
            }
            catch
            {
                continue;
            }
        }
    }

    /// <summary>
    /// Register a service with the specified lifetime
    /// </summary>
    private static void RegisterService(IServiceCollection services, Type serviceType, Type implementationType, ServiceLifetime serviceLifetime)
    {
        switch (serviceLifetime)
        {
            case ServiceLifetime.Singleton:
                services.AddSingleton(serviceType, implementationType);
                break;
            case ServiceLifetime.Transient:
                services.AddTransient(serviceType, implementationType);
                break;
            case ServiceLifetime.Scoped:
            default:
                services.AddScoped(serviceType, implementationType);
                break;
        }
    }
}


