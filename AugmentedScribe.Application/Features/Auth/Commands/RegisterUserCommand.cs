using AugmentedScribe.Application.Features.Auth.Dtos;
using MediatR;

namespace AugmentedScribe.Application.Features.Auth.Commands;

public class RegisterUserCommand(RegisterUserRequest registerUserRequest) : IRequest<AuthResponse>
{
    public RegisterUserRequest RegisterUserRequest { get; } = registerUserRequest;
}