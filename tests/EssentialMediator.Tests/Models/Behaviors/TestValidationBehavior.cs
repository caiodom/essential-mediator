using EssentialMediator.Abstractions.Delegates;
using EssentialMediator.Abstractions.Messages;
using EssentialMediator.Abstractions.Pipelines;
using System.ComponentModel.DataAnnotations;

namespace EssentialMediator.Tests.Models.Behaviors;

public class TestValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public static List<string> ValidationLogs { get; } = new();

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        ValidationLogs.Add($"Validating {requestName}");

        var validationContext = new ValidationContext(request);
        var validationResults = new List<ValidationResult>();

        bool isValid = Validator.TryValidateObject(request, validationContext, validationResults, true);

        if (!isValid)
        {
            var errors = validationResults.Select(x => x.ErrorMessage).ToArray();
            var errorMessage = $"Validation failed for {requestName}: {string.Join(", ", errors)}";
            throw new ValidationException(errorMessage);
        }

        ValidationLogs.Add($"Validation passed for {requestName}");
        
        return await next();
    }
}
