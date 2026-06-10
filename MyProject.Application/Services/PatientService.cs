using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MyProject.Application.DTOs;
using MyProject.Domain.Entities;
using MyProject.Domain.IRepositories;

namespace MyProject.Application.Services;

public class PatientService
{
    private readonly IPatientRepository _repo;
    private readonly IUserRepository _userRepo;
    private readonly IRoleRepository _roleRepo;

    public PatientService(IPatientRepository repo, IUserRepository userRepo, IRoleRepository roleRepo)
    {
        _repo = repo;
        _userRepo = userRepo;
        _roleRepo = roleRepo;
    }

    public async Task<IEnumerable<PatientDto>> GetAllAsync()
    {
        var list = await _repo.GetAllAsync();
        return list.Select(MapToDto);
    }

    public async Task<PatientDto?> GetByIdAsync(int id)
    {
        var p = await _repo.GetByIdAsync(id);
        return p is null ? null : MapToDto(p);
    }

    public async Task CreateAsync(CreatePatientRequest req)
    {
        ValidatePatientData(req.Phone, req.DateOfBirth, req.EmergencyContactPhone);

        // Username must be unique
        var existingUser = await _userRepo.GetByUsernameAsync(req.Username.Trim());
        if (existingUser != null)
        {
            throw new ArgumentException($"Username '{req.Username}' is already taken.");
        }

        // Phone unique validation
        if (!string.IsNullOrWhiteSpace(req.Phone))
        {
            var patients = await _repo.GetAllAsync();
            if (patients.Any(p => p.User.Phone == req.Phone.Trim()))
            {
                throw new ArgumentException($"Phone number '{req.Phone}' is already in use by another patient.");
            }
        }

        // Get Patient role
        var roles = await _roleRepo.GetAllAsync();
        var patientRole = roles.FirstOrDefault(r => r.RoleName.Equals("Patient", StringComparison.OrdinalIgnoreCase))
            ?? throw new KeyNotFoundException("Role 'Patient' not found in system.");

        var user = new User
        {
            FullName = req.FullName.Trim(),
            Username = req.Username.Trim(),
            PasswordHash = HashPassword(req.Password),
            Phone = req.Phone?.Trim(),
            RoleId = patientRole.RoleId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var patient = new Patient
        {
            User = user,
            DateOfBirth = req.DateOfBirth,
            Gender = req.Gender?.Trim(),
            Address = req.Address?.Trim(),
            BloodType = req.BloodType?.Trim(),
            EmergencyContactName = req.EmergencyContactName?.Trim(),
            EmergencyContactPhone = req.EmergencyContactPhone?.Trim()
        };

        await _repo.AddAsync(patient);
    }

    public async Task UpdateAsync(int id, UpdatePatientRequest req)
    {
        ValidatePatientData(req.Phone, req.DateOfBirth, req.EmergencyContactPhone);

        var patient = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Patient with ID {id} not found");

        // Phone unique validation
        if (!string.IsNullOrWhiteSpace(req.Phone))
        {
            var patients = await _repo.GetAllAsync();
            if (patients.Any(p => p.PatientId != id && p.User.Phone == req.Phone.Trim()))
            {
                throw new ArgumentException($"Phone number '{req.Phone}' is already in use by another patient.");
            }
        }

        patient.User.FullName = req.FullName.Trim();
        patient.User.Phone = req.Phone?.Trim();
        patient.User.UpdatedAt = DateTime.UtcNow;

        patient.DateOfBirth = req.DateOfBirth;
        patient.Gender = req.Gender?.Trim();
        patient.Address = req.Address?.Trim();
        patient.BloodType = req.BloodType?.Trim();
        patient.EmergencyContactName = req.EmergencyContactName?.Trim();
        patient.EmergencyContactPhone = req.EmergencyContactPhone?.Trim();

        await _repo.UpdateAsync(patient);
    }

    private void ValidatePatientData(string? phone, DateOnly? dateOfBirth, string? emergencyPhone)
    {
        if (dateOfBirth.HasValue && dateOfBirth.Value > DateOnly.FromDateTime(DateTime.Today))
        {
            throw new ArgumentException("Date of birth cannot be in the future.");
        }

        if (!string.IsNullOrWhiteSpace(phone))
        {
            var p = phone.Trim();
            if (!System.Text.RegularExpressions.Regex.IsMatch(p, @"^0\d{9}$"))
            {
                throw new ArgumentException("Phone number must be a valid 10-digit number starting with 0.");
            }
        }

        if (!string.IsNullOrWhiteSpace(emergencyPhone))
        {
            var ep = emergencyPhone.Trim();
            if (!System.Text.RegularExpressions.Regex.IsMatch(ep, @"^0\d{9}$"))
            {
                throw new ArgumentException("Emergency contact phone number must be a valid 10-digit number starting with 0.");
            }
        }
    }

    public async Task DeleteAsync(int id)
    {
        await _repo.DeleteAsync(id);
    }

    public IQueryable<Patient> GetQueryable()
    {
        return _repo.GetQueryable();
    }

    private PatientDto MapToDto(Patient p)
    {
        return new PatientDto(
            p.PatientId,
            p.UserId,
            p.User.FullName,
            p.User.Username,
            p.User.Phone,
            p.DateOfBirth,
            p.Gender,
            p.Address,
            p.BloodType,
            p.EmergencyContactName,
            p.EmergencyContactPhone
        );
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLower();
    }
}
