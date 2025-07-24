using EssentialMediator.Abstractions.Delegates;
using EssentialMediator.Abstractions.Handlers;
using EssentialMediator.Abstractions.Mediation;
using EssentialMediator.Abstractions.Messages;
using EssentialMediator.Abstractions.Pipelines;
using EssentialMediator.Exceptions;
using EssentialMediator.Mediation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace EssentialMediator
{
    /// <summary>
    /// Default optimized implementation of IMediator
    /// </summary>
    public sealed class Mediator : IMediator
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<Mediator> _logger;
        private static readonly ConcurrentDictionary<Type, MethodInfo> HandleMethodCache = new();

        public Mediator(IServiceProvider serviceProvider,ILogger<Mediator> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        ///<inheritdoc/>
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request,CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            return SendWithPipelineAsync(request, cancellationToken);
        }
        ///<inheritdoc/>
        public Task<Unit> Send(IRequest request,CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            return SendWithPipelineVoidAsync(request, cancellationToken);
        }
        ///<inheritdoc/>
        public async Task Publish(INotification notification,CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(notification);

            var notificationType = notification.GetType();
            var handlerInterface = typeof(INotificationHandler<>).MakeGenericType(notificationType);

            _logger.LogDebug("Publishing notification {NotificationType}", notificationType.Name);

            var handlers = _serviceProvider
                .GetServices(handlerInterface)
                .ToList();

            if (handlers.Count == 0)
            {
                _logger.LogWarning("No handlers registered for notification {NotificationType}",
                    notificationType.Name);

                return;
            }

            _logger.LogDebug("Found {HandlerCount} handlers for notification {NotificationType}",
                             handlers.Count,
                             notificationType.Name);

            var tasks = new List<Task>(handlers.Count);

            foreach (var handler in handlers)
            {
                try
                {
                    var method = HandleMethodCache.GetOrAdd(
                        handler.GetType(),
                        t => t.GetMethod(
                            "Handle",
                            new[] { notificationType, typeof(CancellationToken) }
                        ) ?? throw new HandlerConfigurationException(
                            handlerInterface,
                            "Handle method not found"));

                    if (method.Invoke(handler, new object[] { notification, cancellationToken }) is Task task)
                    {
                        tasks.Add(task);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error creating task for handler {HandlerType}",
                        handler.GetType().Name);
                }
            }

            try
            {
                await Task.WhenAll(tasks).ConfigureAwait(false);

                _logger.LogDebug("Successfully published notification {NotificationType}",
                        notificationType.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Error publishing notification {NotificationType}",
                        notificationType.Name);

                throw;
            }
        }

        private async Task<TResponse> SendWithPipelineAsync<TResponse>(IRequest<TResponse> request,CancellationToken cancellationToken)
        {
            var requestType = request.GetType();
            var responseType = typeof(TResponse);
            var behaviorInterface = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, responseType);
            var behaviors = _serviceProvider.GetServices(behaviorInterface).ToArray();

            if (behaviors.Length == 0)
                return await SendInternalAsync(request, cancellationToken).ConfigureAwait(false);

            _logger.LogDebug("Executing {BehaviorCount} pipeline behaviors for request {RequestType}",
                        behaviors.Length,
                        requestType.Name);

            RequestHandlerDelegate<TResponse> handler =
                () => SendInternalAsync(request, cancellationToken);

            foreach (var behavior in behaviors.AsEnumerable().Reverse())
            {
                var method = HandleMethodCache.GetOrAdd(
                    behavior.GetType(),
                    t => t.GetMethod(
                        "Handle",
                        new[]
                        {
                            requestType,
                            typeof(RequestHandlerDelegate<TResponse>),
                            typeof(CancellationToken)
                        }) ?? throw new HandlerConfigurationException(behaviorInterface,
                                     "Handle method not found"));

                var next = handler;
                handler = () =>
                {
                    if (method.Invoke(behavior,
                            new object[] { request, next, cancellationToken }) is not Task<TResponse> task)
                    {
                        throw new InvalidOperationException("Behavior returned null");
                    }

                    return task;
                };
            }

            return await handler().ConfigureAwait(false);
        }

        private async Task<Unit> SendWithPipelineVoidAsync(IRequest request,CancellationToken cancellationToken)
        {
            var requestType = request.GetType();
            var behaviorInterface = typeof(IPipelineBehavior<,>)
                                     .MakeGenericType(requestType, typeof(Unit));

            var behaviors = _serviceProvider
                .GetServices(behaviorInterface)
                .ToList();

            if (behaviors.Count == 0)
            {
                return await SendInternalVoidAsync(request, cancellationToken)
                    .ConfigureAwait(false);
            }

            _logger.LogDebug(
                "Executing {BehaviorCount} pipeline behaviors for void request {RequestType}",
                behaviors.Count,
                requestType.Name);

            RequestHandlerDelegate<Unit> handler =
                () => SendInternalVoidAsync(request, cancellationToken);

            foreach (var behavior in behaviors.AsEnumerable().Reverse())
            {
                var method = HandleMethodCache.GetOrAdd(
                    behavior.GetType(),
                    t => t.GetMethod(
                        "Handle",
                        new[]
                        {
                            requestType,
                            typeof(RequestHandlerDelegate<Unit>),
                            typeof(CancellationToken)
                        }
                    ) ?? throw new HandlerConfigurationException(
                        behaviorInterface,
                        "Handle method not found"));

                var next = handler;
                handler = () =>
                {
                    if (method.Invoke(
                            behavior,
                            new object[] { request, next, cancellationToken }
                        ) is not Task<Unit> task)
                    {
                        throw new InvalidOperationException("Behavior returned null");
                    }

                    return task;
                };
            }

            return await handler().ConfigureAwait(false);
        }

        private async Task<TResponse> SendInternalAsync<TResponse>(IRequest<TResponse> request,CancellationToken cancellationToken)
        {
            var requestType = request.GetType();
            var responseType = typeof(TResponse);
            var handlerInterface = typeof(IRequestHandler<,>)
                                     .MakeGenericType(requestType, responseType);

            _logger.LogDebug(
                "Sending request {RequestType} expecting {ResponseType}",
                requestType.Name,
                responseType.Name);

            var handlers = _serviceProvider
                .GetServices(handlerInterface)
                .ToArray();

            if (handlers.Length == 0)
            {
                _logger.LogError(
                    "No handler registered for request {RequestType}",
                    requestType.Name);

                throw new HandlerNotFoundException(requestType);
            }

            if (handlers.Length > 1)
            {
                _logger.LogError(
                    "Multiple handlers ({HandlerCount}) found for request {RequestType}",
                    handlers.Length,
                    requestType.Name);

                throw new MultipleHandlersException(requestType, handlers.Length);
            }

            var handler = handlers.Single();

            try
            {
                var method = HandleMethodCache.GetOrAdd(
                    handler.GetType(),
                    t => t.GetMethod(
                        "Handle",
                        new[] { requestType, typeof(CancellationToken) }
                    ) ?? throw new HandlerConfigurationException(
                        handlerInterface,
                        "Handle method not found"));

                if (method.Invoke(
                        handler,
                        new object[] { request, cancellationToken }
                    ) is not Task<TResponse> task)
                {
                    throw new InvalidOperationException($"Handler returned null Task<{responseType.Name}>");
                }

                var result = await task.ConfigureAwait(false);

                _logger.LogDebug("Successfully handled request {RequestType}",requestType.Name);

                return result;
            }
            catch (TargetInvocationException tie) when (tie.InnerException != null)
            {
                _logger.LogError(tie.InnerException,"Error handling request {RequestType}",requestType.Name);

                ExceptionDispatchInfo.Capture(tie.InnerException).Throw();
                throw;
            }
            catch (Exception ex) when (!(ex is MediatorException))
            {
                _logger.LogError(ex,"Error sending request {RequestType}",requestType.Name);
                throw;
            }
        }

        private async Task<Unit> SendInternalVoidAsync(IRequest request,CancellationToken cancellationToken)
        {
            var requestType = request.GetType();
            var handlerInterface = typeof(IRequestHandler<>).MakeGenericType(requestType);

            _logger.LogDebug("Sending void request {RequestType}",requestType.Name);

            var handlers = _serviceProvider.GetServices(handlerInterface).ToArray();
            if (handlers.Length == 0)
            {
                _logger.LogError("No handler registered for request {RequestType}",
                    requestType.Name);

                throw new HandlerNotFoundException(requestType);
            }

            if (handlers.Length > 1)
            {
                _logger.LogError("Multiple handlers ({HandlerCount}) found for request {RequestType}",
                    handlers.Length,
                    requestType.Name);

                throw new MultipleHandlersException(requestType, handlers.Length);
            }

            var handler = handlers.Single();

            try
            {
                var method = HandleMethodCache.GetOrAdd(
                    handler.GetType(),
                    t => t.GetMethod(
                        "Handle",
                        new[] { requestType, typeof(CancellationToken) }
                    ) ?? throw new HandlerConfigurationException(
                        handlerInterface,
                        "Handle method not found"));

                if (method.Invoke(
                        handler,
                        new object[] { request, cancellationToken }
                    ) is not Task<Unit> task)
                {
                    throw new InvalidOperationException(
                        "Handler returned null Task<Unit>");
                }

                var result = await task.ConfigureAwait(false);

                _logger.LogDebug(
                    "Successfully handled void request {RequestType}",
                    requestType.Name);

                return result;
            }
            catch (TargetInvocationException tie) when (tie.InnerException != null)
            {
                _logger.LogError(
                    tie.InnerException,
                    "Error handling void request {RequestType}",
                    requestType.Name);

                ExceptionDispatchInfo.Capture(tie.InnerException).Throw();
                throw;
            }
            catch (Exception ex) when (!(ex is MediatorException))
            {
                _logger.LogError(ex,"Error sending void request {RequestType}",requestType.Name);
                throw;
            }
        }
    }
}
