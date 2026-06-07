using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MyProject.Application.DTOs;
using MyProject.Domain.Entities;
using MyProject.Domain.IRepositories;

namespace MyProject.Application.Services;

public class StaffService
{
    private readonly IStaffRepository _staffRepo;
    private readonly IAccountRepository _accountRepo;

    public StaffService(IStaffRepository staffRepo, IAccountRepository accountRepo)
    {
        _staffRepo = staffRepo;
        _accountRepo = accountRepo;
    }

    public async Task<IEnumerable<StaffDto>> GetAllAsync()
    {
        var list = await _staffRepo.GetAllAsync();
        return list.Select(MapToDto);
    }

    public async Task<StaffDto?> GetByIdAsync(int id)
    {
        var s = await _staffRepo.GetByIdAsync(id);
        return s is null ? null : MapToDto(s);
    }

    public async Task CreateAsync(CreateStaffRequest req)
    {
        var staff = new Staff
        {
            FirstName = req.FirstName,
            LastName = req.LastName,
            Specialization = req.Specialization,
            Phone = req.Phone,
            Email = req.Email,
            Status = req.Status ?? "Active"
        };

        await _staffRepo.AddAsync(staff);

        // Create account if username is provided
        if (!string.IsNullOrWhiteSpace(req.Username) && !string.IsNullOrWhiteSpace(req.Password) && req.RoleId.HasValue)
        {
            var account = new Account
            {
                StaffId = staff.StaffId,
                RoleId = req.RoleId.Value,
                Username = req.Username.Trim(),
                PasswordHash = HashPassword(req.Password),
                IsActive = true,
                CreatedAt = System.DateTime.UtcNow
            };
            await _accountRepo.AddAsync(account);
        }
    }

    public async Task UpdateAsync(int id, UpdateStaffRequest req)
    {
        var staff = await _staffRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Staff with ID {id} not found");

        staff.FirstName = req.FirstName;
        staff.LastName = req.LastName;
        staff.Specialization = req.Specialization;
        staff.Phone = req.Phone;
        staff.Email = req.Email;
        staff.Status = req.Status ?? "Active";

        await _staffRepo.UpdateAsync(staff);

        // Account management
        if (!string.IsNullOrWhiteSpace(req.Username) && req.RoleId.HasValue)
        {
            if (staff.Account != null)
            {
                // Update existing account
                staff.Account.Username = req.Username.Trim();
                staff.Account.RoleId = req.RoleId.Value;
                if (!string.IsNullOrWhiteSpace(req.Password))
                {
                    staff.Account.PasswordHash = HashPassword(req.Password);
                }
                await _accountRepo.UpdateAsync(staff.Account);
            }
            else
            {
                // Create new account
                var account = new Account
                {
                    StaffId = staff.StaffId,
                    RoleId = req.RoleId.Value,
                    Username = req.Username.Trim(),
                    PasswordHash = string.IsNullOrWhiteSpace(req.Password) ? HashPassword("123456") : HashPassword(req.Password),
                    IsActive = true,
                    CreatedAt = System.DateTime.UtcNow
                };
                await _accountRepo.AddAsync(account);
            }
        }
        else if (staff.Account != null)
        {
            // If username is removed, we delete the account
            await _accountRepo.DeleteAsync(staff.Account.AccountId);
        }
    }

    public async Task DeleteAsync(int id)
    {
        var staff = await _staffRepo.GetByIdAsync(id);
        if (staff != null && staff.Account != null)
        {
            await _accountRepo.DeleteAsync(staff.Account.AccountId);
        }
        await _staffRepo.DeleteAsync(id);
    }

    private StaffDto MapToDto(Staff s)
    {
        return new StaffDto(
            s.StaffId,
            s.FirstName,
            s.LastName,
            s.Specialization,
            s.Phone,
            s.Email,
            s.Status,
            s.Account?.AccountId,
            s.Account?.Username,
            s.Account?.RoleId,
            s.Account?.Role?.RoleName
        );
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLower();
    }
}
