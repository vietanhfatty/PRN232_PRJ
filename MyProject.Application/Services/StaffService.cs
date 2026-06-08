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
    private readonly IRoleRepository _roleRepo;

    public StaffService(IStaffRepository staffRepo, IAccountRepository accountRepo, IRoleRepository roleRepo)
    {
        _staffRepo = staffRepo;
        _accountRepo = accountRepo;
        _roleRepo = roleRepo;
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
        // 1. Phone uniqueness check
        var existingPhone = await _staffRepo.GetByPhoneAsync(req.Phone.Trim());
        if (existingPhone != null)
        {
            throw new System.ArgumentException($"Phone number '{req.Phone}' is already in use by another staff member.");
        }

        // 2. Email uniqueness check
        if (!string.IsNullOrWhiteSpace(req.Email))
        {
            var existingEmail = await _staffRepo.GetByEmailAsync(req.Email.Trim());
            if (existingEmail != null)
            {
                throw new System.ArgumentException($"Email address '{req.Email}' is already in use by another staff member.");
            }
        }

        // 3. Username uniqueness check & account rules
        if (!string.IsNullOrWhiteSpace(req.Username))
        {
            var existingAccount = await _accountRepo.GetByUsernameAsync(req.Username.Trim());
            if (existingAccount != null)
            {
                throw new System.ArgumentException($"Username '{req.Username}' is already taken.");
            }

            if (!req.RoleId.HasValue)
            {
                throw new System.ArgumentException("A system role must be selected if a username is provided.");
            }

            if (string.IsNullOrWhiteSpace(req.Password))
            {
                throw new System.ArgumentException("Password is required when creating a new account.");
            }
        }

        // 4. Role-specific validation
        await ValidateRoleAndSpecializationAsync(req.RoleId, req.Specialization);

        var staff = new Staff
        {
            FirstName = req.FirstName.Trim(),
            LastName = req.LastName.Trim(),
            Specialization = string.IsNullOrWhiteSpace(req.Specialization) ? null : req.Specialization.Trim(),
            Phone = req.Phone.Trim(),
            Email = string.IsNullOrWhiteSpace(req.Email) ? null : req.Email.Trim(),
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

        // 1. Phone uniqueness check
        var existingPhone = await _staffRepo.GetByPhoneAsync(req.Phone.Trim());
        if (existingPhone != null && existingPhone.StaffId != id)
        {
            throw new System.ArgumentException($"Phone number '{req.Phone}' is already in use by another staff member.");
        }

        // 2. Email uniqueness check
        if (!string.IsNullOrWhiteSpace(req.Email))
        {
            var existingEmail = await _staffRepo.GetByEmailAsync(req.Email.Trim());
            if (existingEmail != null && existingEmail.StaffId != id)
            {
                throw new System.ArgumentException($"Email address '{req.Email}' is already in use by another staff member.");
            }
        }

        // 3. Username uniqueness check
        if (!string.IsNullOrWhiteSpace(req.Username))
        {
            var existingAccount = await _accountRepo.GetByUsernameAsync(req.Username.Trim());
            if (existingAccount != null && existingAccount.StaffId != id)
            {
                throw new System.ArgumentException($"Username '{req.Username}' is already taken.");
            }

            if (!req.RoleId.HasValue)
            {
                throw new System.ArgumentException("A system role must be selected if a username is provided.");
            }
        }

        // 4. Role-specific validation
        await ValidateRoleAndSpecializationAsync(req.RoleId, req.Specialization);

        staff.FirstName = req.FirstName.Trim();
        staff.LastName = req.LastName.Trim();
        staff.Specialization = string.IsNullOrWhiteSpace(req.Specialization) ? null : req.Specialization.Trim();
        staff.Phone = req.Phone.Trim();
        staff.Email = string.IsNullOrWhiteSpace(req.Email) ? null : req.Email.Trim();
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

    private async Task ValidateRoleAndSpecializationAsync(int? roleId, string? specialization)
    {
        string? roleName = null;
        if (roleId.HasValue)
        {
            var role = await _roleRepo.GetByIdAsync(roleId.Value);
            roleName = role?.RoleName;
        }

        if (roleName != null && roleName.Equals("Doctor", System.StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(specialization))
            {
                throw new System.ArgumentException("Specialization is required for Doctor role.");
            }
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(specialization))
            {
                throw new System.ArgumentException("Specialization is only allowed for Doctor role. Please clear the specialization field.");
            }
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
