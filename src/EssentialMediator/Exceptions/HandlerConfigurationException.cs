namespace EssentialMediator.Exceptions;

/// <summary>
/// Exception thrown when a handler configuration error occurs
/// </summary>
public class HandlerConfigurationException : MediatorException
{
    public Type HandlerType { get; }

    public HandlerConfigurationException(Type handlerType, string message)
        : base($"Handler configuration error for '{handlerType.Name}': {message}")
    {
        HandlerType = handlerType;
    }

    public HandlerConfigurationException(Type handlerType, string message, Exception innerException)
        : base($"Handler configuration error for '{handlerType.Name}': {message}", innerException)
    {
        HandlerType = handlerType;
    }
}
