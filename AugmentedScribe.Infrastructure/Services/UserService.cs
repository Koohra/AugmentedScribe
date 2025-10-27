using System.ComponentModel.DataAnnotations;
using AugmentedScribe.Application.Common.Interfaces;
using AugmentedScribe.Application.Features.Auth.Dtos;
using Microsoft.AspNetCore.Identity;

namespace AugmentedScribe.Infrastructure.Services;

public sealed class UserService(UserManager<IdentityUser> userManager) : IUserService
{
    public async Task<string> RegisterUserAsync(UserDto user, string password)
    {
        var existingUser = await userManager.FindByEmailAsync(user.Email);
        if (existingUser != null)
        {
            throw new ValidationException("User with this email already exists.");
        }

        var newUser = new IdentityUser
        {
            Email = user.Email,
            UserName = user.Email
        };

        var result = await userManager.CreateAsync(newUser, password);

        if (result.Succeeded) return newUser.Id;

        var errors = result.Errors.Select(e => e.Description);
        throw new ValidationException(string.Join("\n", errors));
    }
}