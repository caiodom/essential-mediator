using EssentialMediator.Abstractions.Messages;

namespace EssentialMediator.Tests.Models.Requests;

public class TestRequest : IRequest<string>
{
    public string Message { get; set; } = string.Empty;
}
