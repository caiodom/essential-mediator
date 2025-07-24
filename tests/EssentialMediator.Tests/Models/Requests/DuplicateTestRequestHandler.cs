using EssentialMediator.Abstractions.Handlers;

namespace EssentialMediator.Tests.Models.Requests;

/// <summary>
/// Handler duplicado usado apenas para testes de validação de múltiplos handlers.
/// NÃO registrar automaticamente via assembly scanning.
/// </summary>
public class DuplicateTestRequestHandler : IRequestHandler<TestRequest, string>
{
    public Task<string> Handle(TestRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult($"Duplicate Handler: {request.Message}");
    }
}
