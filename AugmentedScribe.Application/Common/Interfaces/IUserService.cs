using AugmentedScribe.Application.Features.Auth.Dtos;

namespace AugmentedScribe.Application.Common.Interfaces;

public interface IUserService
{
    Task<string> RegisterUserAsync(UserDto user, string password);
}