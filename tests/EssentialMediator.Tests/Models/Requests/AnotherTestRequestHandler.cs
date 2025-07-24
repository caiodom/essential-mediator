using EssentialMediator.Abstractions.Handlers;

namespace EssentialMediator.Tests.Models.Requests;

public class AnotherTestRequestHandler : IRequestHandler<AnotherTestRequest, string>
{
    public Task<string> Handle(AnotherTestRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult($"Another Handler: {request.Message}");
    }
}
