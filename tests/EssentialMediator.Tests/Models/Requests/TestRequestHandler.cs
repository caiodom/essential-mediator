using EssentialMediator.Abstractions.Handlers;

namespace EssentialMediator.Tests.Models.Requests;

public class TestRequestHandler : IRequestHandler<TestRequest, string>
{
    public Task<string> Handle(TestRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult($"Handled: {request.Message}");
    }
}
