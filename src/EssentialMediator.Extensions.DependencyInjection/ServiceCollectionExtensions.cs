using EssentialMediator.Abstractions.Handlers;
using EssentialMediator.Abstractions.Pipelines;
using EssentialMediator.Extensions.DependencyInjection.Configuration;
using EssentialMediator.Extensions.DependencyInjection.Exceptions;
using EssentialMediator.Mediation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

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
    public static IServiceCollection AddEssentialMediator(this IServiceCollection services, params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        assemblies ??= Array.Empty<Assembly>();

        if (assemblies.Length == 0)
        {
            assemblies = new[] { Assembly.GetCallingAssembly() };
        }

        return AddEssentialMediatorCore(
            services,
            assemblies,
            ServiceLifetime.Scoped,
            ServiceLifetime.Scoped);
    }

    /// <summary>
    /// Adds EssentialMediator services to the specified IServiceCollection
    /// </summary>
    public static IServiceCollection AddEssentialMediator(
        this IServiceCollection services,
        Action<MediatorConfiguration> configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var config = new MediatorConfiguration();
        configuration(config);

        return AddEssentialMediatorCore(
            services,
            config.Assemblies.ToArray(),
            config.HandlerLifetime,
            config.MediatorLifetime);
    }

    private static IServiceCollection AddEssentialMediatorCore(
        IServiceCollection services,
        Assembly[] assemblies,
        ServiceLifetime handlerLifetime,
        ServiceLifetime mediatorLifetime)
    {
        ValidateMediatorLifetime(mediatorLifetime);
        RegisterService(services, typeof(IMediator), typeof(Mediator), mediatorLifetime);

        var uniqueAssemblies = new HashSet<Assembly>(assemblies);

        foreach (var assembly in uniqueAssemblies)
        {
            RegisterHandlers(services, assembly, handlerLifetime);
        }

        return services;
    }

    private static void ValidateMediatorLifetime(ServiceLifetime mediatorLifetime)
    {
        if (mediatorLifetime == ServiceLifetime.Singleton)
        {
            throw new InvalidOperationException(
                "EssentialMediator cannot be registered as Singleton when using Microsoft.Extensions.DependencyInjection because the mediator resolves handlers and pipeline behaviors from the ambient service scope. Use Scoped (recommended) or Transient.");
        }
    }

    private static void RegisterHandlers(
        IServiceCollection services,
        Assembly assembly,
        ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        Type[] assemblyTypes;

        try
        {
            assemblyTypes = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            throw new MediatorRegistrationException(assembly, ex);
        }

        var handlerTypes = assemblyTypes
            .Where(type => type.IsClass && !type.IsAbstract && type.IsPublic)
            .ToArray();

        RegisterRequestHandlers(services, handlerTypes, serviceLifetime);
        RegisterNotificationHandlers(services, handlerTypes, serviceLifetime);
    }

    private static void RegisterRequestHandlers(
        IServiceCollection services,
        IEnumerable<Type> types,
        ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        foreach (var type in types)
        {
            var interfaces = type.GetInterfaces()
                .Where(@interface =>
                    @interface.IsGenericType
                    && RequestHandlerTypes.Contains(@interface.GetGenericTypeDefinition()));

            foreach (var @interface in interfaces)
            {
                RegisterService(services, @interface, type, serviceLifetime);
            }
        }
    }

    private static void RegisterNotificationHandlers(
        IServiceCollection services,
        IEnumerable<Type> types,
        ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        foreach (var type in types)
        {
            var interfaces = type.GetInterfaces()
                .Where(@interface =>
                    @interface.IsGenericType
                    && @interface.GetGenericTypeDefinition() == NotificationHandlerType);

            foreach (var @interface in interfaces)
            {
                RegisterService(services, @interface, type, serviceLifetime);
            }
        }
    }

    private static void RegisterService(
        IServiceCollection services,
        Type serviceType,
        Type implementationType,
        ServiceLifetime serviceLifetime)
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
                services.AddScoped(serviceType, implementationType);
                break;
            default:
                services.AddScoped(serviceType, implementationType);
                break;
        }
    }

    #region Pipeline Behavior Extensions

    /// <summary>
    /// Adds a pipeline behavior to the service collection
    /// </summary>
    public static IServiceCollection AddPipelineBehavior<TBehavior>(
        this IServiceCollection services,
        ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        where TBehavior : class
    {
        return AddPipelineBehavior(services, typeof(TBehavior), serviceLifetime);
    }

    /// <summary>
    /// Adds a pipeline behavior to the service collection
    /// </summary>
    public static IServiceCollection AddPipelineBehavior(
        this IServiceCollection services,
        Type behaviorType,
        ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(behaviorType);

        var behaviorInterfaces = behaviorType.GetInterfaces()
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>))
            .ToList();

        if (!behaviorInterfaces.Any())
        {
            throw new ArgumentException(
                $"Type {behaviorType.Name} does not implement IPipelineBehavior<,>",
                nameof(behaviorType));
        }

        foreach (var behaviorInterface in behaviorInterfaces)
        {
            RegisterService(services, behaviorInterface, behaviorType, serviceLifetime);
        }

        return services;
    }

    /// <summary>
    /// Adds built-in logging behavior
    /// </summary>
    public static IServiceCollection AddLoggingBehavior(
        this IServiceCollection services,
        ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        RegisterService(
            services,
            typeof(IPipelineBehavior<,>),
            typeof(EssentialMediator.Behaviors.LoggingBehavior<,>),
            serviceLifetime);
        return services;
    }

    /// <summary>
    /// Adds built-in performance monitoring behavior
    /// </summary>
    public static IServiceCollection AddPerformanceBehavior(
        this IServiceCollection services,
        int slowRequestThresholdMs = 500,
        ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new EssentialMediator.Behaviors.PerformanceBehaviorOptions(slowRequestThresholdMs);
        services.AddSingleton(options);

        RegisterService(
            services,
            typeof(IPipelineBehavior<,>),
            typeof(EssentialMediator.Behaviors.PerformanceBehavior<,>),
            serviceLifetime);
        return services;
    }

    /// <summary>
    /// Adds built-in validation behavior
    /// </summary>
    public static IServiceCollection AddValidationBehavior(
        this IServiceCollection services,
        ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        RegisterService(
            services,
            typeof(IPipelineBehavior<,>),
            typeof(EssentialMediator.Behaviors.ValidationBehavior<,>),
            serviceLifetime);
        return services;
    }

    /// <summary>
    /// Adds all built-in behaviors (Logging, Performance, Validation)
    /// </summary>
    public static IServiceCollection AddAllBuiltInBehaviors(
        this IServiceCollection services,
        int slowRequestThresholdMs = 500,
        ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        return services
            .AddLoggingBehavior(serviceLifetime)
            .AddPerformanceBehavior(slowRequestThresholdMs, serviceLifetime)
            .AddValidationBehavior(serviceLifetime);
    }

    #endregion
}
