using EssentialMediator.Abstractions.Handlers;
using EssentialMediator.Abstractions.Mediation;

namespace EssentialMediator.Tests.Models.Requests;

public class TestVoidRequestHandler : IRequestHandler<TestVoidRequest>
{
    public Task<Unit> Handle(TestVoidRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Unit.Value);
    }
}
