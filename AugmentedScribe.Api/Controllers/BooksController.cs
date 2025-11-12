using System.ComponentModel.DataAnnotations;
using AugmentedScribe.Application.Features.Books.Commands.DeleteBook;
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
        var command = new UploadBookCommand(campaignId, file);
        var bookDto = await _mediator.Send(command);
        return CreatedAtAction(nameof(UploadBook), new { campaignId = bookDto.CampaignId, id = bookDto.Id },
            bookDto);
    }

    [HttpGet("{campaignId:guid}")]
    public async Task<IActionResult> GetBook([FromRoute] Guid campaignId)
    {
        var query = new GetBooksByCampaignQuery(campaignId);
        var books = await _mediator.Send(query);
        return Ok(books);
    }

    [HttpDelete("{campaignId:guid},{bookId:guid}")]
    public async Task<IActionResult> DeleteBook([FromRoute] Guid campaignId, [FromRoute] Guid bookId)
    {
        var command = new DeleteBookCommand(campaignId, bookId);
        await _mediator.Send(command);
        return NoContent();
    }
}