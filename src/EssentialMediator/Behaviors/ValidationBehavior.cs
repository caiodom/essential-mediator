using EssentialMediator.Abstractions.Delegates;
using EssentialMediator.Abstractions.Messages;
using EssentialMediator.Abstractions.Pipelines;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace EssentialMediator.Behaviors;

/// <summary>
/// Validation behavior that validates requests using DataAnnotations
/// </summary>
/// <typeparam name="TRequest">Request type</typeparam>
/// <typeparam name="TResponse">Response type</typeparam>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

    public ValidationBehavior(ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        _logger.LogDebug("Validating request {RequestName}", requestName);

        var validationContext = new ValidationContext(request);
        var validationResults = new List<ValidationResult>();

        bool isValid = Validator.TryValidateObject(request, validationContext, validationResults, true);

        if (!isValid)
        {
            var errors = validationResults.Select(x => x.ErrorMessage).ToArray();
            var errorMessage = $"Validation failed for {requestName}: {string.Join(", ", errors)}";

            _logger.LogWarning("Validation failed for request {RequestName}: {Errors}", requestName, string.Join(", ", errors));

            throw new ValidationException(errorMessage);
        }

        _logger.LogDebug("Request {RequestName} validation passed", requestName);

        return await next();
    }
}
