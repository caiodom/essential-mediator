namespace EssentialMediator.Exceptions;

/// <summary>
/// Exception thrown when no handler is found for a request.
/// </summary>
public class HandlerNotFoundException : MediatorException
{
    /// <summary>
    /// Gets the request type that has no registered handler.
    /// </summary>
    public Type RequestType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="HandlerNotFoundException"/> class.
    /// </summary>
    /// <param name="requestType">Request type that has no registered handler.</param>
    public HandlerNotFoundException(Type requestType)
        : base($"No handler registered for request type '{requestType.Name}'")
    {
        RequestType = requestType;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HandlerNotFoundException"/> class.
    /// </summary>
    /// <param name="requestType">Request type that has no registered handler.</param>
    /// <param name="innerException">Exception that led to the missing-handler error.</param>
    public HandlerNotFoundException(Type requestType, Exception innerException)
        : base($"No handler registered for request type '{requestType.Name}'", innerException)
    {
        RequestType = requestType;
    }
}
