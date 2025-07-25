using EssentialMediator.Abstractions.Messages;
using EssentialMediator.WebApiDemo.Models;
using System.ComponentModel.DataAnnotations;

namespace EssentialMediator.WebApiDemo.Features.Users.Queries;

public class GetUserByIdQuery : IRequest<UserDto?>
{
    [Range(1, int.MaxValue, ErrorMessage = "ID must be greater than zero")]
    public int Id { get; set; }
}
