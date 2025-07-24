using EssentialMediator.Abstractions.Messages;

namespace EssentialMediator.Tests.Models.Requests;

public class TestVoidRequest : IRequest
{
    public string Message { get; set; } = string.Empty;
}
