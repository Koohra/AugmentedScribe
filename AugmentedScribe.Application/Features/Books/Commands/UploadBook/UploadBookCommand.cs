using AugmentedScribe.Application.Features.Books.Dtos;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace AugmentedScribe.Application.Features.Books.Commands.UploadBook;

public record UploadBookCommand(Guid CampaignId, IFormFile File) : IRequest<BookDto>;