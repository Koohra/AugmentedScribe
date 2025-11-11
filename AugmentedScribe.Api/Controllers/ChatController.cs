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
public sealed class ChatController(IMediator mediator, ILogger<ChatController> logger) : ControllerBase
{
    private readonly IMediator _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    private readonly ILogger<ChatController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    [HttpPost]
    public async Task<IActionResult> SendChat(
        [FromRoute] Guid campaignId,
        [FromBody] ChatRequest request)
    {
        try
        {
            var command = new SendChatPromptCommand(campaignId, request);
            var response = await _mediator.Send(command);
            return Ok(response);
        }
        catch (ValidationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An internal error occured while processing the chat.");
            return StatusCode(500, new { message = "An internal error occured while processing the chat." });
        }
    }

    // TODO: Implementar o GET /api/campaigns/{campaignId}/chat (para histórico)
}