namespace EssentialMediator.Exceptions;

/// <summary>
/// Exception thrown when a handler configuration error occurs.
/// </summary>
public class HandlerConfigurationException : MediatorException
{
    /// <summary>
    /// Gets the handler type associated with the configuration error.
    /// </summary>
    public Type HandlerType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="HandlerConfigurationException"/> class.
    /// </summary>
    /// <param name="handlerType">Handler type associated with the error.</param>
    /// <param name="message">Description of the configuration error.</param>
    public HandlerConfigurationException(Type handlerType, string message)
        : base($"Handler configuration error for '{handlerType.Name}': {message}")
    {
        HandlerType = handlerType;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HandlerConfigurationException"/> class.
    /// </summary>
    /// <param name="handlerType">Handler type associated with the error.</param>
    /// <param name="message">Description of the configuration error.</param>
    /// <param name="innerException">Exception that caused the configuration error.</param>
    public HandlerConfigurationException(Type handlerType, string message, Exception innerException)
        : base($"Handler configuration error for '{handlerType.Name}': {message}", innerException)
    {
        HandlerType = handlerType;
    }
}
