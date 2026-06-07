using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyProject.Application.DTOs;
using MyProject.Domain.Entities;
using MyProject.Domain.IRepositories;

namespace MyProject.Application.Services;

public class PatientService
{
    private readonly IPatientRepository _repo;

    public PatientService(IPatientRepository repo)
    {
        _repo = repo;
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
        // Phone must be unique
        var existingPhone = await _repo.GetByPhoneAsync(req.Phone.Trim());
        if (existingPhone != null)
        {
            throw new ArgumentException($"Phone number '{req.Phone}' is already in use by another patient.");
        }

        // InsuranceNo must be unique if provided
        if (!string.IsNullOrWhiteSpace(req.InsuranceNo))
        {
            var existingIns = await _repo.GetByInsuranceNoAsync(req.InsuranceNo.Trim());
            if (existingIns != null)
            {
                throw new ArgumentException($"Insurance number '{req.InsuranceNo}' is already in use by another patient.");
            }
        }

        var patient = new Patient
        {
            FirstName = req.FirstName.Trim(),
            LastName = req.LastName.Trim(),
            Gender = req.Gender,
            Dob = req.Dob,
            Phone = req.Phone.Trim(),
            Address = req.Address?.Trim(),
            InsuranceNo = string.IsNullOrWhiteSpace(req.InsuranceNo) ? null : req.InsuranceNo.Trim()
        };

        await _repo.AddAsync(patient);
    }

    public async Task UpdateAsync(int id, UpdatePatientRequest req)
    {
        var patient = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Patient with ID {id} not found");

        // Phone unique validation
        var existingPhone = await _repo.GetByPhoneAsync(req.Phone.Trim());
        if (existingPhone != null && existingPhone.PatientId != id)
        {
            throw new ArgumentException($"Phone number '{req.Phone}' is already in use by another patient.");
        }

        // InsuranceNo unique validation
        if (!string.IsNullOrWhiteSpace(req.InsuranceNo))
        {
            var existingIns = await _repo.GetByInsuranceNoAsync(req.InsuranceNo.Trim());
            if (existingIns != null && existingIns.PatientId != id)
            {
                throw new ArgumentException($"Insurance number '{req.InsuranceNo}' is already in use by another patient.");
            }
        }

        patient.FirstName = req.FirstName.Trim();
        patient.LastName = req.LastName.Trim();
        patient.Gender = req.Gender;
        patient.Dob = req.Dob;
        patient.Phone = req.Phone.Trim();
        patient.Address = req.Address?.Trim();
        patient.InsuranceNo = string.IsNullOrWhiteSpace(req.InsuranceNo) ? null : req.InsuranceNo.Trim();

        await _repo.UpdateAsync(patient);
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
            p.FirstName,
            p.LastName,
            p.Gender,
            p.Dob,
            p.Phone,
            p.Address,
            p.InsuranceNo
        );
    }
}
