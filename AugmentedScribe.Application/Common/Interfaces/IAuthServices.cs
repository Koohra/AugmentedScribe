namespace AugmentedScribe.Application.Common.Interfaces;

public class LoginResult
{
    public bool Succeeded { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public interface IAuthServices
{
    Task<LoginResult> LoginAsync(string email, string password);
}