using EssentialMediator.Abstractions.Messages;

namespace EssentialMediator.Tests.Models.Requests;

public class AnotherTestRequest : IRequest<string>
{
    public string Message { get; set; } = string.Empty;
}

