using EssentialMediator.Abstractions.Messages;
using EssentialMediator.WebApiDemo.Models;

namespace EssentialMediator.WebApiDemo.Features.Users.Queries;

public class GetAllUsersQuery : IRequest<List<UserDto>>
{
    public bool? IsActive { get; set; }
    public string? SearchTerm { get; set; }
}
