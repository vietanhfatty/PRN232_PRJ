using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MyProject.Application.DTOs;
using MyProject.Domain.IRepositories;

namespace MyProject.Application.Services;

public class AuthService
{
    private readonly IAccountRepository _accountRepo;
    private readonly IStaffRepository _staffRepo;

    public AuthService(IAccountRepository accountRepo, IStaffRepository staffRepo)
    {
        _accountRepo = accountRepo;
        _staffRepo = staffRepo;
    }

    public async Task<LoginResponse> LoginAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return new LoginResponse(false, "Username and password are required.", null, null, null, null);
        }

        var account = await _accountRepo.GetByUsernameAsync(username.Trim());
        if (account == null)
        {
            return new LoginResponse(false, "Invalid username or password.", null, null, null, null);
        }

        if (!account.IsActive)
        {
            return new LoginResponse(false, "This account is inactive.", null, null, null, null);
        }

        var inputHash = HashPassword(password);
        if (account.PasswordHash != inputHash)
        {
            return new LoginResponse(false, "Invalid username or password.", null, null, null, null);
        }

        var staff = await _staffRepo.GetByIdAsync(account.StaffId);
        if (staff == null)
        {
            return new LoginResponse(false, "Staff details not found for this account.", null, null, null, null);
        }

        var fullName = $"{staff.FirstName} {staff.LastName}";
        var roleName = staff.Account?.Role?.RoleName ?? "Staff";

        return new LoginResponse(
            true,
            "Login successful.",
            staff.StaffId,
            account.Username,
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
