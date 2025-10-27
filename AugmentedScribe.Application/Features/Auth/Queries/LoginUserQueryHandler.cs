using System.ComponentModel.DataAnnotations;
using AugmentedScribe.Application.Common.Interfaces;
using AugmentedScribe.Application.Features.Auth.Dtos;
using MediatR;

namespace AugmentedScribe.Application.Features.Auth.Queries;

public sealed class LoginUserQueryHandler(IAuthServices authServices, IJwtTokenGenerator jwtTokenGenerator)
    : IRequestHandler<LoginUserQuery, AuthResponse>
{
    private readonly IAuthServices
        _authServices = authServices ?? throw new ArgumentNullException(nameof(authServices));

    private readonly IJwtTokenGenerator _jwtTokenGenerator =
        jwtTokenGenerator ?? throw new ArgumentNullException(nameof(jwtTokenGenerator));

    public async Task<AuthResponse> Handle(LoginUserQuery query, CancellationToken cancellationToken)
    {
        var request = query.LoginUserRequest;
        
        var loginResult = await _authServices.LoginAsync(request.Email, request.Password);

        if (!loginResult.Succeeded)
        {
            throw new ValidationException("Email or password is incorrect");
        }
        
        var token = _jwtTokenGenerator.GenerateToken(loginResult.UserId, loginResult.Email);

        return new AuthResponse
        {
            UserId = loginResult.UserId,
            Email = loginResult.Email,
            Token = token
        };
    }
}