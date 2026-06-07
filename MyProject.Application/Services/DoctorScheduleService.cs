using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyProject.Application.DTOs;
using MyProject.Domain.Entities;
using MyProject.Domain.IRepositories;

namespace MyProject.Application.Services;

public class DoctorScheduleService
{
    private readonly IDoctorScheduleRepository _repo;
    private readonly IStaffRepository _staffRepo;

    public DoctorScheduleService(IDoctorScheduleRepository repo, IStaffRepository staffRepo)
    {
        _repo = repo;
        _staffRepo = staffRepo;
    }

    public async Task<IEnumerable<DoctorScheduleDto>> GetAllAsync()
    {
        var list = await _repo.GetAllAsync();
        return list.Select(MapToDto);
    }

    public async Task<DoctorScheduleDto?> GetByIdAsync(int id)
    {
        var ds = await _repo.GetByIdAsync(id);
        return ds is null ? null : MapToDto(ds);
    }

    public async Task CreateAsync(CreateDoctorScheduleRequest req)
    {
        // Check doctor exists
        var doctor = await _staffRepo.GetByIdAsync(req.DoctorId)
            ?? throw new KeyNotFoundException($"Staff/Doctor with ID {req.DoctorId} not found");

        // Validate shift name
        var shift = req.ShiftName.Trim();
        if (shift != "Morning" && shift != "Afternoon" && shift != "Night")
        {
            throw new ArgumentException("ShiftName must be 'Morning', 'Afternoon', or 'Night'.");
        }

        // Validate overlap
        var existing = await _repo.GetByDoctorDateShiftAsync(req.DoctorId, req.WorkDate, shift);
        if (existing != null)
        {
            throw new ArgumentException($"Doctor schedule already exists for doctor '{doctor.FirstName} {doctor.LastName}' on {req.WorkDate:yyyy-MM-dd} during shift '{shift}'.");
        }

        var schedule = new DoctorSchedule
        {
            DoctorId = req.DoctorId,
            WorkDate = req.WorkDate,
            ShiftName = shift,
            MaxPatients = req.MaxPatients ?? 20
        };

        await _repo.AddAsync(schedule);
    }

    public async Task UpdateAsync(int id, UpdateDoctorScheduleRequest req)
    {
        var schedule = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Doctor schedule with ID {id} not found");

        // Check doctor exists
        var doctor = await _staffRepo.GetByIdAsync(req.DoctorId)
            ?? throw new KeyNotFoundException($"Staff/Doctor with ID {req.DoctorId} not found");

        // Validate shift name
        var shift = req.ShiftName.Trim();
        if (shift != "Morning" && shift != "Afternoon" && shift != "Night")
        {
            throw new ArgumentException("ShiftName must be 'Morning', 'Afternoon', or 'Night'.");
        }

        // Validate overlap
        var existing = await _repo.GetByDoctorDateShiftAsync(req.DoctorId, req.WorkDate, shift);
        if (existing != null && existing.ScheduleId != id)
        {
            throw new ArgumentException($"Doctor schedule already exists for doctor '{doctor.FirstName} {doctor.LastName}' on {req.WorkDate:yyyy-MM-dd} during shift '{shift}'.");
        }

        schedule.DoctorId = req.DoctorId;
        schedule.WorkDate = req.WorkDate;
        schedule.ShiftName = shift;
        schedule.MaxPatients = req.MaxPatients ?? 20;

        await _repo.UpdateAsync(schedule);
    }

    public async Task DeleteAsync(int id)
    {
        await _repo.DeleteAsync(id);
    }

    private DoctorScheduleDto MapToDto(DoctorSchedule ds)
    {
        return new DoctorScheduleDto(
            ds.ScheduleId,
            ds.DoctorId,
            ds.Doctor != null ? $"{ds.Doctor.FirstName} {ds.Doctor.LastName}" : "Unknown Doctor",
            ds.WorkDate,
            ds.ShiftName,
            ds.MaxPatients
        );
    }
}
