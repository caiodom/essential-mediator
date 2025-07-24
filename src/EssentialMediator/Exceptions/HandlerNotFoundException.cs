namespace EssentialMediator.Exceptions;

/// <summary>
/// Exception thrown when no handler is found for a request
/// </summary>
public class HandlerNotFoundException : MediatorException
{
    public Type RequestType { get; }

    public HandlerNotFoundException(Type requestType)
        : base($"No handler registered for request type '{requestType.Name}'")
    {
        RequestType = requestType;
    }

    public HandlerNotFoundException(Type requestType, Exception innerException)
        : base($"No handler registered for request type '{requestType.Name}'", innerException)
    {
        RequestType = requestType;
    }
}
