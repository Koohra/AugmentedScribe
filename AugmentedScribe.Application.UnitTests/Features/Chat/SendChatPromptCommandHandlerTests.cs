using AugmentedScribe.Application.Common.Interfaces;
using AugmentedScribe.Application.Features.Chat.Commands;
using AugmentedScribe.Application.Features.Chat.Dtos;
using AugmentedScribe.Domain.Entities;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using System.ComponentModel.DataAnnotations;

namespace AugmentedScribe.Application.UnitTests.Features.Chat;

public class SendChatPromptCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IChatService _chatService;
    private readonly ICampaignRepository _campaignRepository;
    private readonly SendChatPromptCommandHandler _handler;

    private readonly string _testUserId = Guid.NewGuid().ToString();
    private readonly Guid _testCampaignId = Guid.NewGuid();
    private readonly Campaign _existingCampaign;

    public SendChatPromptCommandHandlerTests()
    {
        // Mocks
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _chatService = Substitute.For<IChatService>();
        _campaignRepository = Substitute.For<ICampaignRepository>();

        _unitOfWork.Campaigns.Returns(_campaignRepository);

        _handler = new SendChatPromptCommandHandler(_currentUserService, _unitOfWork, _chatService);

        _currentUserService.GetUserId().Returns(_testUserId);

        _existingCampaign = new Campaign { Id = _testCampaignId, UserId = _testUserId };
        _campaignRepository.GetCampaignByIdAsync(_testCampaignId).Returns(_existingCampaign);
    }

    private SendChatPromptCommand CreateValidCommand(string prompt = "Test prompt")
    {
        return new SendChatPromptCommand(_testCampaignId, new ChatRequest { Prompt = prompt });
    }

    [Fact]
    public async Task Handle_ShouldReturnChatResponse_WhenRequestIsValid()
    {
        // Arrange
        var command = CreateValidCommand();
        var aiResponse = "This is a test response from the AI.";

        _chatService.GenerateResponseAsync(_existingCampaign, command.Request.Prompt, CancellationToken.None)
            .Returns(aiResponse);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ChatResponse>();
        result.Response.Should().Be(aiResponse);

        _currentUserService.Received(1).GetUserId();
        await _campaignRepository.Received(1).GetCampaignByIdAsync(_testCampaignId);
        await _chatService.Received(1).GenerateResponseAsync(
            _existingCampaign,
            command.Request.Prompt,
            CancellationToken.None);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var command = CreateValidCommand();
        _currentUserService.GetUserId().ReturnsNull();

        // Act
        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("User is not authenticated.");

        await _chatService.DidNotReceive().GenerateResponseAsync(default, default, default);
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenCampaignNotFound()
    {
        // Arrange
        var command = CreateValidCommand();
        _campaignRepository.GetCampaignByIdAsync(_testCampaignId).ReturnsNull();

        // Act
        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("Campaign not found.");

        await _chatService.DidNotReceive().GenerateResponseAsync(default, default, default);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenUserIsNotCampaignOwner()
    {
        // Arrange
        var command = CreateValidCommand();
        _existingCampaign.UserId = Guid.NewGuid().ToString();

        // Act
        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("User is not authorized to access this campaign.");

        await _chatService.DidNotReceive().GenerateResponseAsync(default, default, default);
    }
}