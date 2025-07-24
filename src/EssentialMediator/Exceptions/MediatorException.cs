namespace EssentialMediator.Exceptions;

/// <summary>
/// Base exception for EssentialMediator
/// </summary>
public abstract class MediatorException : Exception
{
    protected MediatorException(string message) : base(message)
    {
    }

    protected MediatorException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
