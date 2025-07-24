using EssentialMediator.Abstractions.Handlers;

namespace EssentialMediator.Tests.Models.Requests;

public class DuplicateTestRequestHandler : IRequestHandler<TestRequest, string>
{
    public Task<string> Handle(TestRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult($"Duplicate Handler: {request.Message}");
    }
}
