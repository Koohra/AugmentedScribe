using System.ComponentModel.DataAnnotations;
using AugmentedScribe.Application.Common.Interfaces;
using AugmentedScribe.Application.Features.Campaigns.Commands;
using AugmentedScribe.Domain.Entities;
using FluentAssertions;
using MediatR;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace AugmentedScribe.Application.UnitTests.Features.Campaigns;

public sealed class UpdateCampaignCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICampaignRepository _campaignRepository;
    private readonly UpdateCampaignCommandHandler _handler;

    private readonly string _testUserId = Guid.NewGuid().ToString();
    private readonly Guid _testCampaignId = Guid.NewGuid();
    private readonly Campaign _existingCampaign;

    public UpdateCampaignCommandHandlerTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _campaignRepository = Substitute.For<ICampaignRepository>();
        _unitOfWork.Campaigns.Returns(_campaignRepository);
        _handler = new UpdateCampaignCommandHandler(_currentUserService, _unitOfWork);
        _currentUserService.GetUserId().Returns(_testUserId);
        
        _existingCampaign = new Campaign
        {
            Id = _testCampaignId,
            UserId = _testUserId,
            Name = "Original Name",
            System = "Original System",
            Description = "Original Description"
        };
        _campaignRepository.GetCampaignByIdAsync(_testCampaignId).Returns(_existingCampaign);
    }
    
    [Fact]
    public async Task Handle_ShouldUpdateCampaign_WhenUserIsOwner()
    {
        // Arrange
        var command = CreateValidUpdateCommand();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        await _campaignRepository.Received(1).GetCampaignByIdAsync(_testCampaignId);
        _existingCampaign.Name.Should().Be(command.Name);
        _existingCampaign.Description.Should().Be(command.Description);
        _existingCampaign.System.Should().Be(command.System);
        await _unitOfWork.Received(1).CompleteAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var command = CreateValidUpdateCommand();
        _currentUserService.GetUserId().ReturnsNull();

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("No user authentication");

        await _unitOfWork.DidNotReceive().CompleteAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenCampaignNotFound()
    {
        // Arrange
        var command = CreateValidUpdateCommand();
        _campaignRepository.GetCampaignByIdAsync(_testCampaignId).ReturnsNull();

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("Campaign not found");

        await _unitOfWork.DidNotReceive().CompleteAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenUserIsNotOwner()
    {
        // Arrange
        var command = CreateValidUpdateCommand();
        var anotherUserId = Guid.NewGuid().ToString();
        _existingCampaign.UserId = anotherUserId;

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("User is not authorized to update this campaign.");

        await _unitOfWork.DidNotReceive().CompleteAsync(CancellationToken.None);
    }
    
    private UpdateCampaignCommand CreateValidUpdateCommand()
    {
        return new UpdateCampaignCommand(
            _testCampaignId,
            "Updated Name",
            "Updated Description",
            "Updated System"
        );
    }
}