namespace EssentialMediator.Exceptions;

/// <summary>
/// Base exception for EssentialMediator.
/// </summary>
public abstract class MediatorException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MediatorException"/> class.
    /// </summary>
    /// <param name="message">Message that describes the error.</param>
    protected MediatorException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MediatorException"/> class.
    /// </summary>
    /// <param name="message">Message that describes the error.</param>
    /// <param name="innerException">Exception that caused the current error.</param>
    protected MediatorException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
