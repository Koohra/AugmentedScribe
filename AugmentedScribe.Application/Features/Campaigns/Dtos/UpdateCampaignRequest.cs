using System.ComponentModel.DataAnnotations;

namespace AugmentedScribe.Application.Features.Campaigns.Dtos;

public sealed record UpdateCampaignRequest
{
    [Required(ErrorMessage = "Name is required")]
    [MaxLength(100, ErrorMessage = "Name must be less than 100 characters")]
    public string Name { get; init; } = string.Empty;

    [Required(ErrorMessage = "System is required")]
    [MaxLength(50, ErrorMessage = "System must be less than 50 characters")]
    public string System { get; init; } = string.Empty;

    [MaxLength(500, ErrorMessage = "Description must be less than 500 characters")]
    public string? Description { get; init; }
}