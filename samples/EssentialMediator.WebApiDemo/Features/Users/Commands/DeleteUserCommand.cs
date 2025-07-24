using EssentialMediator.Abstractions.Mediation;
using EssentialMediator.Abstractions.Messages;
using System.ComponentModel.DataAnnotations;

namespace EssentialMediator.WebApiDemo.Features.Users.Commands;

public class DeleteUserCommand : IRequest<Unit>
{
    [Range(1, int.MaxValue, ErrorMessage = "ID must be greater than zero")]
    public int Id { get; set; }
}
