using AugmentedScribe.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace AugmentedScribe.Infrastructure.Services;

public sealed class AuthService(SignInManager<IdentityUser> singInManager, UserManager<IdentityUser> userManager)
    : IAuthServices
{
    private readonly SignInManager<IdentityUser> _singInManager =
        singInManager ?? throw new ArgumentNullException(nameof(singInManager));

    private readonly UserManager<IdentityUser> _userManager =
        userManager ?? throw new ArgumentNullException(nameof(userManager));

    public async Task<LoginResult> LoginAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return new LoginResult { Succeeded = false };
        }

        var singInResult = await _singInManager.CheckPasswordSignInAsync(user, password, false);
        if (!singInResult.Succeeded)
        {
            return new LoginResult { Succeeded = false };
        }

        return new LoginResult
        {
            Succeeded = true,
            UserId = user.Id,
            Email = user.Email!
        };
    }
}