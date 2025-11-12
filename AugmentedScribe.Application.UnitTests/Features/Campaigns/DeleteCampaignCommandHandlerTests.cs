using AugmentedScribe.Application.Common.Interfaces;
using AugmentedScribe.Application.Features.Campaigns.Commands;
using AugmentedScribe.Domain.Entities;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using System.ComponentModel.DataAnnotations;
using MediatR;

namespace AugmentedScribe.Application.UnitTests.Features.Campaigns;

public class DeleteCampaignCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICampaignRepository _campaignRepository;
    private readonly DeleteCampaignCommandHandler _handler;

    private readonly string _testUserId = Guid.NewGuid().ToString();
    private readonly Guid _testCampaignId = Guid.NewGuid();

    public DeleteCampaignCommandHandlerTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _campaignRepository = Substitute.For<ICampaignRepository>();
        _unitOfWork.Campaigns.Returns(_campaignRepository);
        _handler = new DeleteCampaignCommandHandler(_currentUserService, _unitOfWork);
        _currentUserService.GetUserId().Returns(_testUserId);
    }

    [Fact]
    public async Task Handle_ShouldDeleteCampaign_WhenUserIsOwner()
    {
        // Arrange
        var command = new DeleteCampaignCommand(_testCampaignId);
        var campaign = new Campaign { Id = _testCampaignId, UserId = _testUserId };
        _campaignRepository.GetCampaignByIdAsync(_testCampaignId).Returns(campaign);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        await _campaignRepository.Received(1).GetCampaignByIdAsync(_testCampaignId);
        _campaignRepository.Received(1).DeleteCampaign(campaign);
        await _unitOfWork.Received(1).CompleteAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var command = new DeleteCampaignCommand(_testCampaignId);
        _currentUserService.GetUserId().ReturnsNull();

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("No user authentication");
        _campaignRepository.DidNotReceive().DeleteCampaign(Arg.Any<Campaign>());
        await _unitOfWork.DidNotReceive().CompleteAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenCampaignNotFound()
    {
        // Arrange
        var command = new DeleteCampaignCommand(_testCampaignId);
        _campaignRepository.GetCampaignByIdAsync(_testCampaignId).ReturnsNull();

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("Campaign not found");
        _campaignRepository.DidNotReceive().DeleteCampaign(Arg.Any<Campaign>());
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenUserIsNotOwner()
    {
        // Arrange
        var command = new DeleteCampaignCommand(_testCampaignId);
        var anotherUserId = Guid.NewGuid().ToString();
        var campaign = new Campaign { Id = _testCampaignId, UserId = anotherUserId };
        _campaignRepository.GetCampaignByIdAsync(_testCampaignId).Returns(campaign);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("User is not authorized to update this campaign.");
        _campaignRepository.DidNotReceive().DeleteCampaign(Arg.Any<Campaign>());
    }
}