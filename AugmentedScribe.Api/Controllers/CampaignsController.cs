using System.ComponentModel.DataAnnotations;
using AugmentedScribe.Application.Features.Campaigns.Commands;
using AugmentedScribe.Application.Features.Campaigns.Dtos;
using AugmentedScribe.Application.Features.Campaigns.Queries;
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

    [HttpGet]
    public async Task<IActionResult> GetCampaigns()
    {
        try
        {
            var query = new GetCampaignQuery();
            var campaignDtos = await _mediator.Send(query);
            return Ok(campaignDtos);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "Error internal" });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCampaign([FromRoute] Guid id)
    {
        try
        {
            var query = new GetCampaignByIdQuery(id);
            var campaignDto = await _mediator.Send(query);
            return Ok(campaignDto);
        }
        catch (ValidationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An internal error occured while fetching the campaign." });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCampaign([FromRoute] Guid id, [FromBody] UpdateCampaignRequest request)
    {
        try
        {
            var command = new UpdateCampaignCommand(id, request.Name, request.Description, request.System);
            await _mediator.Send(command);
            return NoContent();
        }
        catch (ValidationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An internal error occured while updating the campaign." });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCampaign([FromRoute] Guid id)
    {
        try
        {
            var command = new DeleteCampaignCommand(id);
            await _mediator.Send(command);
            return NoContent();
        }
        catch (ValidationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An internal error occured while deleting the campaign." });
        }
    }
}