using System.ComponentModel.DataAnnotations;

namespace AugmentedScribe.Application.Features.Chat.Dtos;

public sealed record ChatRequest
{
    [Required]
    public string Prompt { get; init; } = string.Empty;
}