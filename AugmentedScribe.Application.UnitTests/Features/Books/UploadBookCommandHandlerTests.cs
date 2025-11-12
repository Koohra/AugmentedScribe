using System.ComponentModel.DataAnnotations;
using AugmentedScribe.Application.Common.Interfaces;
using AugmentedScribe.Application.Features.Books.Commands.UploadBook;
using AugmentedScribe.Application.Features.Books.Dtos;
using AugmentedScribe.Contracts;
using AugmentedScribe.Domain.Entities;
using FluentAssertions;
using MassTransit;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace AugmentedScribe.Application.UnitTests.Features.Books;

public class UploadBookCommandHandlerTests
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ICampaignRepository _campaignRepository;
    private readonly IBookRepository _bookRepository;
    private readonly UploadBookCommandHandler _handler;

    private readonly string _testUserId = Guid.NewGuid().ToString();
    private readonly Guid _testCampaignId = Guid.NewGuid();
    private readonly Campaign _existingCampaign;

    public UploadBookCommandHandlerTests()
    {
        _currentUserService = Substitute.For<ICurrentUserService>();
        _fileStorageService = Substitute.For<IFileStorageService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _publishEndpoint = Substitute.For<IPublishEndpoint>();
        _campaignRepository = Substitute.For<ICampaignRepository>();
        _bookRepository = Substitute.For<IBookRepository>();
        _unitOfWork.Campaigns.Returns(_campaignRepository);
        _unitOfWork.Books.Returns(_bookRepository);

        _handler = new UploadBookCommandHandler(
            _currentUserService,
            _fileStorageService,
            _unitOfWork,
            _publishEndpoint);

        _currentUserService.GetUserId().Returns(_testUserId);
        _existingCampaign = new Campaign { Id = _testCampaignId, UserId = _testUserId };
        _campaignRepository.GetCampaignByIdAsync(_testCampaignId).Returns(_existingCampaign);
    }

    private IFormFile CreateMockFile(string contentType, long length)
    {
        var file = Substitute.For<IFormFile>();
        file.ContentType.Returns(contentType);
        file.Length.Returns(length);
        file.FileName.Returns("test.pdf");
        file.OpenReadStream().Returns(new MemoryStream());
        return file;
    }

    [Fact]
    public async Task Handle_ShouldUploadBook_WhenRequestIsValid()
    {
        // Arrange
        var file = CreateMockFile("application/pdf", 1024);
        var command = new UploadBookCommand(_testCampaignId, file);
        var fakeFileUrl = "http://fake.storage.url/book.pdf";

        _fileStorageService.UploadFileAsync(Arg.Any<Stream>(), Arg.Any<string>(), CancellationToken.None)
            .Returns(fakeFileUrl);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<BookDto>();
        result.FileName.Should().Be("test.pdf");
        result.StorageUrl.Should().Be(fakeFileUrl);
        result.Status.Should().Be(AugmentedScribe.Domain.Enums.BookStatus.Pending);

        _currentUserService.Received(1).GetUserId();
        await _campaignRepository.Received(1).GetCampaignByIdAsync(_testCampaignId);
        await _fileStorageService.Received(1)
            .UploadFileAsync(Arg.Any<Stream>(), Arg.Any<string>(), CancellationToken.None);
        _bookRepository.Received(1).Add(Arg.Is<Book>(b => b.StorageUrl == fakeFileUrl));
        await _unitOfWork.Received(1).CompleteAsync(CancellationToken.None);
        await _publishEndpoint.Received(1).Publish(Arg.Is<BookUploadedEvent>(e => e.StorageUrl == fakeFileUrl),
            CancellationToken.None);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var file = CreateMockFile("application/pdf", 1024);
        var command = new UploadBookCommand(_testCampaignId, file);

        _currentUserService.GetUserId().ReturnsNull();

        // Act
        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("No user authentication");
        await _fileStorageService.DidNotReceive().UploadFileAsync(default, default, default);
        _bookRepository.DidNotReceive().Add(default);
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenFileIsEmpty()
    {
        // Arrange
        var file = CreateMockFile("application/pdf", 0);
        var command = new UploadBookCommand(_testCampaignId, file);

        // Act
        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("File is required");
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenFileIsNotPdf()
    {
        // Arrange
        var file = CreateMockFile("image/png", 1024);
        var command = new UploadBookCommand(_testCampaignId, file);

        // Act
        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("File must be pdf");
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenCampaignNotFound()
    {
        // Arrange
        var file = CreateMockFile("application/pdf", 1024);
        var command = new UploadBookCommand(_testCampaignId, file);

        _campaignRepository.GetCampaignByIdAsync(_testCampaignId).ReturnsNull();

        // Act
        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("Campaign not found");
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenUserIsNotCampaignOwner()
    {
        // Arrange
        var file = CreateMockFile("application/pdf", 1024);
        var command = new UploadBookCommand(_testCampaignId, file);

        _existingCampaign.UserId = Guid.NewGuid().ToString();

        // Act
        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("User not authorized");
    }
}