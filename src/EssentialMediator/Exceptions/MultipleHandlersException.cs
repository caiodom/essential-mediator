namespace EssentialMediator.Exceptions;

/// <summary>
/// Exception thrown when multiple handlers are found for a request that expects a single handler.
/// </summary>
public class MultipleHandlersException : MediatorException
{
    /// <summary>
    /// Gets the request type that has multiple registered handlers.
    /// </summary>
    public Type RequestType { get; }

    /// <summary>
    /// Gets the number of handlers that were found.
    /// </summary>
    public int HandlerCount { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MultipleHandlersException"/> class.
    /// </summary>
    /// <param name="requestType">Request type that has multiple registered handlers.</param>
    /// <param name="handlerCount">Number of handlers that were found.</param>
    public MultipleHandlersException(Type requestType, int handlerCount)
        : base($"Multiple handlers ({handlerCount}) found for request type '{requestType.Name}' that expects single handler")
    {
        RequestType = requestType;
        HandlerCount = handlerCount;
    }
}
