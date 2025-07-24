using EssentialMediator.Abstractions.Handlers;
using EssentialMediator.Tests.Models.Requests;

namespace EssentialMediator.Tests.Models.Requests;

public class ValidatedRequestHandler : IRequestHandler<ValidatedRequest, string>
{
    public Task<string> Handle(ValidatedRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult($"Validated: {request.Message}");
    }
}
