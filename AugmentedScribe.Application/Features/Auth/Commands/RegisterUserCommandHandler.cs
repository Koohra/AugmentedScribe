using AugmentedScribe.Application.Common.Interfaces;
using AugmentedScribe.Application.Features.Auth.Dtos;
using MediatR;

namespace AugmentedScribe.Application.Features.Auth.Commands;

public sealed class RegisterUserCommandHandler(IJwtTokenGenerator jwtTokenGenerator, IUserService userService)
    : IRequestHandler<RegisterUserCommand, AuthResponse>
{
    private readonly IJwtTokenGenerator _jwtTokenGenerator =
        jwtTokenGenerator ?? throw new ArgumentNullException(nameof(jwtTokenGenerator));

    private readonly IUserService _userService =
        userService ?? throw new ArgumentNullException(nameof(userService));


    public async Task<AuthResponse> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var request = command.RegisterUserRequest;

        var userDto = new UserDto
        {
            Email = request.Email
        };

        var newUserId = await _userService.RegisterUserAsync(userDto, request.Password);

        var token = _jwtTokenGenerator.GenerateToken(newUserId, request.Email);

        return new AuthResponse
        {
            UserId = newUserId,
            Email = request.Email,
            Token = token
        };
    }
}