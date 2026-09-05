using System.Reflection;

namespace EssentialMediator.Extensions.DependencyInjection.Exceptions;

/// <summary>
/// Represents a failure while scanning an assembly for mediator services.
/// </summary>
public sealed class MediatorRegistrationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MediatorRegistrationException"/> class.
    /// </summary>
    public MediatorRegistrationException(Assembly assembly, ReflectionTypeLoadException innerException)
        : base(BuildMessage(assembly, innerException), innerException)
    {
        Assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
        LoaderExceptions = innerException.LoaderExceptions
            .Where(exception => exception is not null)
            .Cast<Exception>()
            .ToArray();
    }

    /// <summary>
    /// Gets the assembly that could not be scanned completely.
    /// </summary>
    public Assembly Assembly { get; }

    /// <summary>
    /// Gets the underlying loader exceptions reported by the runtime.
    /// </summary>
    public IReadOnlyList<Exception> LoaderExceptions { get; }

    private static string BuildMessage(Assembly assembly, ReflectionTypeLoadException exception)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentNullException.ThrowIfNull(exception);

        var loaderMessages = exception.LoaderExceptions
            .Where(loaderException => loaderException is not null)
            .Select(loaderException => loaderException!.Message)
            .Distinct()
            .ToArray();

        var detail = loaderMessages.Length == 0
            ? "No loader exception details were provided."
            : string.Join(" | ", loaderMessages);

        return $"Failed to scan assembly '{assembly.FullName}' for EssentialMediator handlers. {detail}";
    }
}
