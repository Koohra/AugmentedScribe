using AugmentedScribe.Application.Common.Interfaces;
using AugmentedScribe.Application.Features.Campaigns.Dtos;
using AugmentedScribe.Application.Features.Campaigns.Queries;
using AugmentedScribe.Domain.Entities;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using System.ComponentModel.DataAnnotations;

namespace AugmentedScribe.Application.UnitTests.Features.Campaigns;

public class GetCampaignByIdQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICampaignRepository _campaignRepository;
    private readonly GetCampaignByIdQueryHandler _handler;

    private readonly string _testUserId = Guid.NewGuid().ToString();
    private readonly Guid _testCampaignId = Guid.NewGuid();
    private readonly Campaign _existingCampaign;

    public GetCampaignByIdQueryHandlerTests()
    {
        // Mocks
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _campaignRepository = Substitute.For<ICampaignRepository>();

        _unitOfWork.Campaigns.Returns(_campaignRepository);
        _handler = new GetCampaignByIdQueryHandler(_unitOfWork, _currentUserService);
        
        _currentUserService.GetUserId().Returns(_testUserId);
        
        _existingCampaign = new Campaign
        {
            Id = _testCampaignId,
            UserId = _testUserId,
            Name = "Test Campaign",
            System = "D&D 5e"
        };
        _campaignRepository.GetCampaignByIdAsync(_testCampaignId).Returns(_existingCampaign);
    }

    [Fact]
    public async Task Handle_ShouldReturnCampaignDto_WhenUserIsOwner()
    {
        // Arrange (Happy Path)
        var query = new GetCampaignByIdQuery(_testCampaignId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<CampaignDto>();
        result.Id.Should().Be(_testCampaignId);
        result.Name.Should().Be(_existingCampaign.Name);
        
        await _campaignRepository.Received(1).GetCampaignByIdAsync(_testCampaignId);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var query = new GetCampaignByIdQuery(_testCampaignId);
        _currentUserService.GetUserId().ReturnsNull(); // Usuário não logado

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("User not authorized");
        
        await _campaignRepository.DidNotReceive().GetCampaignByIdAsync(Arg.Any<Guid>());
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenCampaignNotFound()
    {
        // Arrange
        var query = new GetCampaignByIdQuery(_testCampaignId);
        _campaignRepository.GetCampaignByIdAsync(_testCampaignId).ReturnsNull(); // Campanha não existe

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("Campaign not found");
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenUserIsNotOwner()
    {
        // Arrange
        var query = new GetCampaignByIdQuery(_testCampaignId);
        _existingCampaign.UserId = Guid.NewGuid().ToString(); 

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("User not authorized");
    }
}