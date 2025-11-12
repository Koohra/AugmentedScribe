using AugmentedScribe.Application.Common.Interfaces;
using AugmentedScribe.Application.Features.Campaigns.Commands;
using AugmentedScribe.Application.Features.Campaigns.Dtos;
using AugmentedScribe.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace AugmentedScribe.Application.UnitTests.Features.Campaigns;

public sealed class CreateCampaignCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICampaignRepository _campaignRepository;
    private readonly CreateCampaignCommandHandler _handler;

    public CreateCampaignCommandHandlerTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _campaignRepository = Substitute.For<ICampaignRepository>();
        _unitOfWork.Campaigns.Returns(_campaignRepository);

        _handler = new CreateCampaignCommandHandler(_unitOfWork, _currentUserService);
    }

    [Fact]
    public async Task Handle_ShouldCreateCampaign_WhenUserIsAuthenticated()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var command = CreateValidCommand();
        _currentUserService.GetUserId().Returns(userId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(command.CreateRequest.Name);
        result.System.Should().Be(command.CreateRequest.System);
        result.Description.Should().Be(command.CreateRequest.Description);
        result.Id.Should().NotBeEmpty();

        _currentUserService.Received(1).GetUserId();
        _campaignRepository.Received(1).AddCampaign(Arg.Is<Campaign>(c => c.UserId == userId));
        await _unitOfWork.Received(1).CompleteAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenDatabaseSaveFails()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var command = CreateValidCommand(); 

        _currentUserService.GetUserId().Returns(userId);
        _unitOfWork.CompleteAsync(CancellationToken.None)
            .Returns(Task.FromException<int>(new Exception("Database error")));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Database error");
        
        _campaignRepository.Received(1).AddCampaign(Arg.Is<Campaign>(c => c.UserId == userId));
    }

    private static CreateCampaignCommand CreateValidCommand()
    {
        return new CreateCampaignCommand(new CreateCampaignRequest
        {
            Name = "Test Campaign",
            System = "D&D 5e",
            Description = "Test Description"
        });
    }
}