using System.ComponentModel.DataAnnotations;
using AugmentedScribe.Application.Features.Books.Commands.UploadBook;
using AugmentedScribe.Application.Features.Books.Queries.GetBooksByCampaign;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AugmentedScribe.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class BooksController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

    [HttpPost("{campaignId:guid}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadBook([FromRoute] Guid campaignId, [FromForm] IFormFile file)
    {
        try
        {
            var command = new UploadBookCommand(campaignId, file);
            var bookDto = await _mediator.Send(command);
            return CreatedAtAction(nameof(UploadBook), new { campaignId = bookDto.CampaignId, id = bookDto.Id },
                bookDto);
        }
        catch (ValidationException ex)
        {
            if (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { message = ex.Message });
            }

            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An internal error occured while upload a book." });
        }
    }

    [HttpGet("{campaignId:guid}")]
    public async Task<IActionResult> GetBook([FromRoute] Guid campaignId)
    {
        try
        {
            var query = new GetBooksByCampaignQuery(campaignId);
            var books = await _mediator.Send(query);
            return Ok(books);
        }
        catch (ValidationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An internal error occured while upload a book." });
        }
    }
}