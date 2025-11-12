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
        var command = new CreateCampaignCommand(request);
        var campaignDto = await _mediator.Send(command);
        return CreatedAtAction(nameof(CreateCampaign), new { id = campaignDto.Id }, campaignDto);
    }

    [HttpGet]
    public async Task<IActionResult> GetCampaigns()
    {
        var query = new GetCampaignQuery();
        var campaignDtos = await _mediator.Send(query);
        return Ok(campaignDtos);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCampaign([FromRoute] Guid id)
    {
        var query = new GetCampaignByIdQuery(id);
        var campaignDto = await _mediator.Send(query);
        return Ok(campaignDto);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCampaign([FromRoute] Guid id, [FromBody] UpdateCampaignRequest request)
    {
        var command = new UpdateCampaignCommand(id, request.Name, request.Description, request.System);
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCampaign([FromRoute] Guid id)
    {
        var command = new DeleteCampaignCommand(id);
        await _mediator.Send(command);
        return NoContent();
    }
}