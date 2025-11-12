using System.ComponentModel.DataAnnotations;
using AugmentedScribe.Application.Features.Chat.Commands;
using AugmentedScribe.Application.Features.Chat.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AugmentedScribe.Controllers;

[ApiController]
[Route("api/campaigns/{campaignId:guid}/[controller]")]
[Authorize]
public sealed class ChatController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

    [HttpPost]
    public async Task<IActionResult> SendChat(
        [FromRoute] Guid campaignId,
        [FromBody] ChatRequest request)
    {
        var command = new SendChatPromptCommand(campaignId, request);
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    // TODO: Implementar o GET /api/campaigns/{campaignId}/chat (para histórico)
}