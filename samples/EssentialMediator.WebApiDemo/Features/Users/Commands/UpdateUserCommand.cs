using EssentialMediator.Abstractions.Messages;
using EssentialMediator.WebApiDemo.Models;
using System.ComponentModel.DataAnnotations;

namespace EssentialMediator.WebApiDemo.Features.Users.Commands;

public class UpdateUserCommand : IRequest<UserDto>
{
    [Range(1, int.MaxValue, ErrorMessage = "ID must be greater than zero")]
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Email must be a valid format")]
    [StringLength(200, ErrorMessage = "Email must be at most 200 characters")]
    public string Email { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}
