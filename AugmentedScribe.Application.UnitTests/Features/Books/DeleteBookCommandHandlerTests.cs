using AugmentedScribe.Application.Common.Interfaces;
using AugmentedScribe.Application.Features.Books.Commands.DeleteBook;
using AugmentedScribe.Domain.Entities;
using FluentAssertions;
using MediatR;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using System.ComponentModel.DataAnnotations;

namespace AugmentedScribe.Application.UnitTests.Features.Books;

public class DeleteBookCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICampaignRepository _campaignRepository;
    private readonly IBookRepository _bookRepository;
    private readonly DeleteBookCommandHandler _handler;

    private readonly string _testUserId = Guid.NewGuid().ToString();
    private readonly Guid _testCampaignId = Guid.NewGuid();
    private readonly Guid _testBookId = Guid.NewGuid();
    private readonly Campaign _existingCampaign;
    private readonly Book _existingBook;

    public DeleteBookCommandHandlerTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _fileStorageService = Substitute.For<IFileStorageService>();
        _campaignRepository = Substitute.For<ICampaignRepository>();
        _bookRepository = Substitute.For<IBookRepository>();

        _unitOfWork.Campaigns.Returns(_campaignRepository);
        _unitOfWork.Books.Returns(_bookRepository);
        
        _handler = new DeleteBookCommandHandler(_unitOfWork, _fileStorageService, _currentUserService);
        
        _currentUserService.GetUserId().Returns(_testUserId);

        _existingCampaign = new Campaign { Id = _testCampaignId, UserId = _testUserId };
        _campaignRepository.GetCampaignByIdAsync(_testCampaignId).Returns(_existingCampaign);

        _existingBook = new Book
        {
            Id = _testBookId,
            CampaignId = _testCampaignId,
            StorageUrl = "http://fake.url/books/test.pdf"
        };
        _bookRepository.GetBookByIdAsync(_testBookId, CancellationToken.None).Returns(_existingBook);
    }

    [Fact]
    public async Task Handle_ShouldDeleteBook_WhenRequestIsValid()
    {
        // Arrange (Happy Path)
        var command = new DeleteBookCommand(_testCampaignId, _testBookId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        
        await _campaignRepository.Received(1).GetCampaignByIdAsync(_testCampaignId);
        await _bookRepository.Received(1).GetBookByIdAsync(_testBookId, CancellationToken.None);
        await _fileStorageService.Received(1).DeleteFileAsync(Arg.Any<string>(), CancellationToken.None);
        _bookRepository.Received(1).Delete(_existingBook);
        await _unitOfWork.Received(1).CompleteAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var command = new DeleteBookCommand(_testCampaignId, _testBookId);
        _currentUserService.GetUserId().ReturnsNull();

        // Act
        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("User is not authenticated.");

        await _fileStorageService.DidNotReceive().DeleteFileAsync(Arg.Any<string>(), CancellationToken.None);
        _bookRepository.DidNotReceive().Delete(Arg.Any<Book>());
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenUserIsNotCampaignOwner()
    {
        // Arrange
        var command = new DeleteBookCommand(_testCampaignId, _testBookId);
        _existingCampaign.UserId = Guid.NewGuid().ToString();

        // Act
        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("User is not authorized to modify this campaign.");

        await _fileStorageService.DidNotReceive().DeleteFileAsync(Arg.Any<string>(), CancellationToken.None);
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenBookNotFound()
    {
        // Arrange
        var command = new DeleteBookCommand(_testCampaignId, _testBookId);
        _bookRepository.GetBookByIdAsync(_testBookId, CancellationToken.None).ReturnsNull();

        // Act
        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("Book not found");

        await _fileStorageService.DidNotReceive().DeleteFileAsync(Arg.Any<string>(), CancellationToken.None);
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenBookDoesNotBelongToCampaign()
    {
        // Arrange
        var command = new DeleteBookCommand(_testCampaignId, _testBookId);

        var wrongCampaignBook = new Book
        {
            Id = _testBookId,
            CampaignId = Guid.NewGuid(),
            StorageUrl = "http://fake.url/books/test.pdf"
        };

        _bookRepository.GetBookByIdAsync(_testBookId, CancellationToken.None).Returns(wrongCampaignBook);

        // Act
        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("Book does not belong to this campaign.");

        await _fileStorageService.DidNotReceive().DeleteFileAsync(Arg.Any<string>(), CancellationToken.None);
    }
}