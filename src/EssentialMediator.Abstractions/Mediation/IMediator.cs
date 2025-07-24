using EssentialMediator.Abstractions.Mediation;
using EssentialMediator.Abstractions.Messages;

namespace EssentialMediator.Mediation;

/// <summary>
/// Defines a mediator to encapsulate request/response and publisher/subscriber patterns
/// </summary>
public interface IMediator
{
    /// <summary>
    /// Send a request to a single handler
    /// </summary>
    /// <typeparam name="TResponse">Response type</typeparam>
    /// <param name="request">Request object</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>Response object</returns>
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send a request to a single handler with void response
    /// </summary>
    /// <param name="request">Request object</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    Task<Unit> Send(IRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously send a notification to multiple handlers
    /// </summary>
    /// <param name="notification">Notification object</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    Task Publish(INotification notification, CancellationToken cancellationToken = default);

}
