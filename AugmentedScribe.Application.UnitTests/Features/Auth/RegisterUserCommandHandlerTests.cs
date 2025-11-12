using AugmentedScribe.Application.Common.Interfaces;
using AugmentedScribe.Application.Features.Auth.Commands;
using AugmentedScribe.Application.Features.Auth.Dtos;
using FluentAssertions;
using NSubstitute;
using System.ComponentModel.DataAnnotations;

namespace AugmentedScribe.Application.UnitTests.Features.Auth;

public class RegisterUserCommandHandlerTests
{
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IUserService _userService;
    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserCommandHandlerTests()
    {
        _jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
        _userService = Substitute.For<IUserService>();

        _handler = new RegisterUserCommandHandler(_jwtTokenGenerator, _userService);
    }

    [Fact]
    public async Task Handle_ShouldReturnAuthResponse_WhenRegistrationIsSuccessful()
    {
        // Arrange
        var command = CreateValidCommand();
        var newUserId = Guid.NewGuid().ToString();
        const string fakeToken = "fake.jwt.token";

        _userService.RegisterUserAsync(
                Arg.Is<UserDto>(u => u.Email == command.RegisterUserRequest.Email),
                command.RegisterUserRequest.Password)
            .Returns(newUserId);

        _jwtTokenGenerator.GenerateToken(newUserId, command.RegisterUserRequest.Email)
            .Returns(fakeToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<AuthResponse>();
        result.UserId.Should().Be(newUserId);
        result.Email.Should().Be(command.RegisterUserRequest.Email);
        result.Token.Should().Be(fakeToken);

        await _userService.Received(1).RegisterUserAsync(Arg.Any<UserDto>(), Arg.Any<string>());
        _jwtTokenGenerator.Received(1).GenerateToken(newUserId, command.RegisterUserRequest.Email);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenUserServiceFails()
    {
        // Arrange
        var command = CreateValidCommand();
        var exception = new ValidationException("User with this email already exists.");

        _userService.RegisterUserAsync(Arg.Any<UserDto>(), Arg.Any<string>())
            .Returns(Task.FromException<string>(exception));

        // Act
        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("User with this email already exists.");
        _jwtTokenGenerator.DidNotReceive().GenerateToken(Arg.Any<string>(), Arg.Any<string>());
    }

    private static RegisterUserCommand CreateValidCommand()
    {
        var request = new RegisterUserRequest
        {
            Email = "test@user.com",
            Password = "password123"
        };
        return new RegisterUserCommand(request);
    }
}