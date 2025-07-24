namespace EssentialMediator.Exceptions;

/// <summary>
/// Exception thrown when multiple handlers are found for a request that expects single handler
/// </summary>
public class MultipleHandlersException : MediatorException
{
    public Type RequestType { get; }
    public int HandlerCount { get; }

    public MultipleHandlersException(Type requestType, int handlerCount)
        : base($"Multiple handlers ({handlerCount}) found for request type '{requestType.Name}' that expects single handler")
    {
        RequestType = requestType;
        HandlerCount = handlerCount;
    }
}
