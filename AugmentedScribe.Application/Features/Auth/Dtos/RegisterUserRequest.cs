using System.ComponentModel.DataAnnotations;

namespace AugmentedScribe.Application.Features.Auth.Dtos;

public sealed class RegisterUserRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } =  string.Empty;
}