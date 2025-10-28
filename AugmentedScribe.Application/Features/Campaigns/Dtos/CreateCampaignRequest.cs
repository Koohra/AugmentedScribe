using System.ComponentModel.DataAnnotations;

namespace AugmentedScribe.Application.Features.Campaigns.Dtos;

public sealed class CreateCampaignRequest
{
    [Required] [MaxLength(100)] public string Name { get; set; } = string.Empty;
    [Required] [MaxLength(50)] public string System { get; set; } = string.Empty;
    [MaxLength(500)] public string? Description { get; set; }
}