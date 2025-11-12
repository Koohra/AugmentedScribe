using AugmentedScribe.Application.Common.Interfaces;
using AugmentedScribe.Application.Features.Books.Dtos;
using AugmentedScribe.Application.Features.Books.Queries.GetBooksByCampaign;
using AugmentedScribe.Domain.Entities;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using System.ComponentModel.DataAnnotations;

namespace AugmentedScribe.Application.UnitTests.Features.Books;

public sealed class GetBooksByCampaignQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICampaignRepository _campaignRepository;
    private readonly IBookRepository _bookRepository;
    private readonly GetBooksByCampaignQueryHandler _handler;

    private readonly string _testUserId = Guid.NewGuid().ToString();
    private readonly Guid _testCampaignId = Guid.NewGuid();
    private readonly Campaign _existingCampaign;

    public GetBooksByCampaignQueryHandlerTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _campaignRepository = Substitute.For<ICampaignRepository>();
        _bookRepository = Substitute.For<IBookRepository>();

        _unitOfWork.Campaigns.Returns(_campaignRepository);
        _unitOfWork.Books.Returns(_bookRepository);

        _handler = new GetBooksByCampaignQueryHandler(_unitOfWork, _currentUserService);

        _currentUserService.GetUserId().Returns(_testUserId);
        _existingCampaign = new Campaign { Id = _testCampaignId, UserId = _testUserId };
        _campaignRepository.GetCampaignByIdAsync(_testCampaignId).Returns(_existingCampaign);
    }

    [Fact]
    public async Task Handle_ShouldReturnBookDtoList_WhenRequestIsValid()
    {
        // Arrange
        var query = new GetBooksByCampaignQuery(_testCampaignId);
        var books = new List<Book>
        {
            new() { Id = Guid.NewGuid(), FileName = "Book 1", CampaignId = _testCampaignId },
            new() { Id = Guid.NewGuid(), FileName = "Book 2", CampaignId = _testCampaignId }
        };

        _bookRepository.GetBooksByCampaignIdAsync(_testCampaignId, CancellationToken.None).Returns(books);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        var resultList = result.ToList();

        // Assert
        result.Should().NotBeNull();
        resultList.Count.Should().Be(2);
        resultList[0].FileName.Should().Be("Book 1");
        resultList.Should().AllBeOfType<BookDto>();

        _currentUserService.Received(1).GetUserId();
        await _campaignRepository.Received(1).GetCampaignByIdAsync(_testCampaignId);
        await _bookRepository.Received(1).GetBooksByCampaignIdAsync(_testCampaignId, CancellationToken.None);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenCampaignHasNoBooks()
    {
        // Arrange
        var query = new GetBooksByCampaignQuery(_testCampaignId);

        _bookRepository.GetBooksByCampaignIdAsync(_testCampaignId, CancellationToken.None)
            .Returns(new List<Book>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var query = new GetBooksByCampaignQuery(_testCampaignId);
        _currentUserService.GetUserId().ReturnsNull();

        // Act
        Func<Task> act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("No user authentication");

        await _campaignRepository.DidNotReceive().GetCampaignByIdAsync(Arg.Any<Guid>());
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenCampaignNotFound()
    {
        // Arrange
        var query = new GetBooksByCampaignQuery(_testCampaignId);
        _campaignRepository.GetCampaignByIdAsync(_testCampaignId).ReturnsNull();

        // Act
        Func<Task> act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("Campaign not found");

        await _bookRepository.DidNotReceive().GetBooksByCampaignIdAsync(Arg.Any<Guid>(), CancellationToken.None);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenUserIsNotCampaignOwner()
    {
        // Arrange
        var query = new GetBooksByCampaignQuery(_testCampaignId);
        _existingCampaign.UserId = Guid.NewGuid().ToString();

        // Act
        Func<Task> act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("User not found");

        await _bookRepository.DidNotReceive().GetBooksByCampaignIdAsync(Arg.Any<Guid>(), CancellationToken.None);
    }
}