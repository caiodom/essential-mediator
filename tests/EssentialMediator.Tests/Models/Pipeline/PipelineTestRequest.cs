using EssentialMediator.Abstractions.Messages;

namespace EssentialMediator.Tests.Models.Pipeline;

public class PipelineTestRequest : IRequest<string>
{
    public string Message { get; set; } = string.Empty;
}
