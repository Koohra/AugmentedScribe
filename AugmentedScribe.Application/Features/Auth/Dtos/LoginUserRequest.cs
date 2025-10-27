using System.ComponentModel.DataAnnotations;

namespace AugmentedScribe.Application.Features.Auth.Dtos;

public sealed class LoginUserRequest
{
    [Required] [EmailAddress] public string Email { get; set; } = string.Empty;

    [Required] public string Password { get; set; } = string.Empty;
}