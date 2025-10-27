using AugmentedScribe.Application.Features.Auth.Dtos;

namespace AugmentedScribe.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(string userId, string email);
}
