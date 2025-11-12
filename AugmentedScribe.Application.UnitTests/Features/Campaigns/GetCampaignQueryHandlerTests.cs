using AugmentedScribe.Application.Common.Interfaces;
using AugmentedScribe.Application.Features.Campaigns.Dtos;
using AugmentedScribe.Application.Features.Campaigns.Queries;
using AugmentedScribe.Domain.Entities;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace AugmentedScribe.Application.UnitTests.Features.Campaigns;

public class GetCampaignQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICampaignRepository _campaignRepository;
    private readonly GetCampaignQueryHandler _handler;

    private readonly string _testUserId = Guid.NewGuid().ToString();

    public GetCampaignQueryHandlerTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _campaignRepository = Substitute.For<ICampaignRepository>();

        _unitOfWork.Campaigns.Returns(_campaignRepository);
        
        _handler = new GetCampaignQueryHandler(_unitOfWork, _currentUserService);
        
        _currentUserService.GetUserId().Returns(_testUserId);
    }

    [Fact]
    public async Task Handle_ShouldReturnCampaignDtoList_WhenUserHasCampaigns()
    {
        // Arrange
        var query = new GetCampaignQuery();
        var campaigns = new List<Campaign>
        {
            new() { Id = Guid.NewGuid(), Name = "Campaign 1", UserId = _testUserId, System = "D&D" },
            new() { Id = Guid.NewGuid(), Name = "Campaign 2", UserId = _testUserId, System = "CoC" }
        };
        
        _campaignRepository.GetCampaignsByUserIdAsync(_testUserId).Returns(campaigns);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        var resultList = result.ToList(); // Converte IEnumerable para lista

        // Assert
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<IEnumerable<CampaignDto>>();
        resultList.Count.Should().Be(2);
        resultList[0].Name.Should().Be("Campaign 1");
        resultList[1].System.Should().Be("CoC");
        
        await _campaignRepository.Received(1).GetCampaignsByUserIdAsync(_testUserId);
    }
    
    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenUserHasNoCampaigns()
    {
        // Arrange
        var query = new GetCampaignQuery();
        
        _campaignRepository.GetCampaignsByUserIdAsync(_testUserId).Returns(new List<Campaign>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        var resultList = result.ToList();

        // Assert
        result.Should().NotBeNull();
        resultList.Should().BeEmpty();
        
        await _campaignRepository.Received(1).GetCampaignsByUserIdAsync(_testUserId);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var query = new GetCampaignQuery();
        _currentUserService.GetUserId().ReturnsNull(); // Usuário não logado

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("User no authentication");
        
        await _campaignRepository.DidNotReceive().GetCampaignsByUserIdAsync(Arg.Any<string>());
    }
}