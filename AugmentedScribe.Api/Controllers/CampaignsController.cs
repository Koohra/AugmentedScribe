using System.ComponentModel.DataAnnotations;
using AugmentedScribe.Application.Features.Campaigns.Commands;
using AugmentedScribe.Application.Features.Campaigns.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AugmentedScribe.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class CampaignsController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

    [HttpPost]
    public async Task<IActionResult> CreateCampaign([FromBody] CreateCampaignRequest request)
    {
        try
        {
            var command = new CreateCampaignCommand(request);
            var campaignDto = await _mediator.Send(command);
            return CreatedAtAction(nameof(CreateCampaign), new { id = campaignDto.Id }, campaignDto);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "Error internal" });
        }
    }
}