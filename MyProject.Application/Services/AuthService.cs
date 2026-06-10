using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MyProject.Application.DTOs;
using MyProject.Domain.Entities;
using MyProject.Domain.IRepositories;

namespace MyProject.Application.Services;

public class AuthService
{
    private readonly IUserRepository _userRepo;

    public AuthService(IUserRepository userRepo)
    {
        _userRepo = userRepo;
    }

    public async Task<LoginResponse> LoginAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return new LoginResponse(false, "Username and password are required.", null, null, null, null);
        }

        var user = await _userRepo.GetByUsernameAsync(username.Trim());
        if (user == null)
        {
            return new LoginResponse(false, "Invalid username or password.", null, null, null, null);
        }

        if (!user.IsActive)
        {
            return new LoginResponse(false, "This account is inactive.", null, null, null, null);
        }

        var inputHash = HashPassword(password);
        if (user.PasswordHash != inputHash)
        {
            return new LoginResponse(false, "Invalid username or password.", null, null, null, null);
        }

        var fullName = user.FullName;
        var roleName = user.Role?.RoleName ?? "User";

        return new LoginResponse(
            true,
            "Login successful.",
            user.UserId,
            user.Username,
            roleName,
            fullName
        );
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLower();
    }
}
