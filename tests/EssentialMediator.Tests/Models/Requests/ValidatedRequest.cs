using EssentialMediator.Abstractions.Messages;
using System.ComponentModel.DataAnnotations;

namespace EssentialMediator.Tests.Models.Requests;

public class ValidatedRequest : IRequest<string>
{
    [Required(ErrorMessage = "Message is required")]
    [MinLength(3, ErrorMessage = "Message must be at least 3 characters")]
    public string Message { get; set; } = string.Empty;
}
