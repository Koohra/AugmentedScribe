using AugmentedScribe.Application.Common.Interfaces;
using AugmentedScribe.Application.Features.Auth.Dtos;
using AugmentedScribe.Application.Features.Auth.Queries;
using FluentAssertions;
using NSubstitute;
using System.ComponentModel.DataAnnotations;

namespace AugmentedScribe.Application.UnitTests.Features.Auth;

public class LoginUserQueryHandlerTests
{
    private readonly IAuthServices _authServices;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly LoginUserQueryHandler _handler;

    public LoginUserQueryHandlerTests()
    {
        _authServices = Substitute.For<IAuthServices>();
        _jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
        
        _handler = new LoginUserQueryHandler(_authServices, _jwtTokenGenerator);
    }

    private static LoginUserQuery CreateValidQuery()
    {
        var request = new LoginUserRequest
        {
            Email = "test@user.com",
            Password = "password123"
        };
        return new LoginUserQuery(request);
    }

    [Fact]
    public async Task Handle_ShouldReturnAuthResponse_WhenLoginIsSuccessful()
    {
        // Arrange
        var query = CreateValidQuery();
        var loginResult = new LoginResult 
        { 
            Succeeded = true, 
            UserId = Guid.NewGuid().ToString(), 
            Email = query.LoginUserRequest.Email 
        };
        const string fakeToken = "fake.jwt.token";
        
        _authServices.LoginAsync(query.LoginUserRequest.Email, query.LoginUserRequest.Password)
            .Returns(loginResult);
        _jwtTokenGenerator.GenerateToken(loginResult.UserId, loginResult.Email)
            .Returns(fakeToken);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be(fakeToken);
        result.UserId.Should().Be(loginResult.UserId);

        await _authServices.Received(1).LoginAsync(Arg.Any<string>(), Arg.Any<string>());
        _jwtTokenGenerator.Received(1).GenerateToken(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenLoginFails()
    {
        // Arrange
        var query = CreateValidQuery();
        var loginResult = new LoginResult { Succeeded = false };

        _authServices.LoginAsync(query.LoginUserRequest.Email, query.LoginUserRequest.Password)
            .Returns(loginResult);

        // Act
        Func<Task> act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("Email or password is incorrect");
        
        _jwtTokenGenerator.DidNotReceive().GenerateToken(Arg.Any<string>(), Arg.Any<string>());
    }
}