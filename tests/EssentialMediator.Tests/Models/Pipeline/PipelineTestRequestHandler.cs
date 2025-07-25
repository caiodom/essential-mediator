using EssentialMediator.Abstractions.Handlers;

namespace EssentialMediator.Tests.Models.Pipeline;

public class PipelineTestRequestHandler : IRequestHandler<PipelineTestRequest, string>
{
    public Task<string> Handle(PipelineTestRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult($"Pipeline Handled: {request.Message}");
    }
}
