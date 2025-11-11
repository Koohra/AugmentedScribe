namespace AugmentedScribe.Application.Features.Chat.Dtos;

public sealed record ChatResponse
{
    public string Response { get; init; } = string.Empty;
}