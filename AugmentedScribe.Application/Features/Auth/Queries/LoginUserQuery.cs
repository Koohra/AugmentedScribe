using AugmentedScribe.Application.Features.Auth.Dtos;
using MediatR;

namespace AugmentedScribe.Application.Features.Auth.Queries;

public sealed class LoginUserQuery(LoginUserRequest loginUserRequest) : IRequest<AuthResponse>
{
    public LoginUserRequest LoginUserRequest { get; } = loginUserRequest;
}