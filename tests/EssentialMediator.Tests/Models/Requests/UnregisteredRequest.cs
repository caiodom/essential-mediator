using EssentialMediator.Abstractions.Messages;

namespace EssentialMediator.Tests.Models.Requests;

public class UnregisteredRequest : IRequest<string>
{
    public string Message { get; set; } = string.Empty;
}
