using EssentialMediator.Abstractions.Mediation;

namespace EssentialMediator.Abstractions.Messages;

/// <summary>
/// Marker interface to represent a request with a response
/// </summary>
/// <typeparam name="TResponse">Response type</typeparam>
public interface IRequest<out TResponse>
{
}

/// <summary>
/// Represents a void response
/// </summary>
public interface IRequest : IRequest<Unit>
{
}

