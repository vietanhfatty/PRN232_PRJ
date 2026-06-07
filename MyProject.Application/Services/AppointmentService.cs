using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyProject.Application.DTOs;
using MyProject.Domain.Entities;
using MyProject.Domain.IRepositories;

namespace MyProject.Application.Services;

public class AppointmentService
{
    private readonly IAppointmentRepository _repo;
    private readonly IPatientRepository _patientRepo;
    private readonly IStaffRepository _staffRepo;

    public AppointmentService(
        IAppointmentRepository repo,
        IPatientRepository patientRepo,
        IStaffRepository staffRepo)
    {
        _repo = repo;
        _patientRepo = patientRepo;
        _staffRepo = staffRepo;
    }

    public async Task<IEnumerable<AppointmentDto>> GetAllAsync()
    {
        var list = await _repo.GetAllAsync();
        return list.Select(MapToDto);
    }

    public async Task<AppointmentDto?> GetByIdAsync(int id)
    {
        var a = await _repo.GetByIdAsync(id);
        return a is null ? null : MapToDto(a);
    }

    public async Task CreateAsync(CreateAppointmentRequest req)
    {
        // Check patient exists
        var patient = await _patientRepo.GetByIdAsync(req.PatientId)
            ?? throw new KeyNotFoundException($"Patient with ID {req.PatientId} not found");

        // Check doctor exists
        var doctor = await _staffRepo.GetByIdAsync(req.DoctorId)
            ?? throw new KeyNotFoundException($"Staff/Doctor with ID {req.DoctorId} not found");

        var appointment = new Appointment
        {
            PatientId = req.PatientId,
            DoctorId = req.DoctorId,
            AppointmentDate = req.AppointmentDate,
            Type = string.IsNullOrWhiteSpace(req.Type) ? "First Visit" : req.Type.Trim(),
            Status = string.IsNullOrWhiteSpace(req.Status) ? "Scheduled" : req.Status.Trim(),
            Reason = req.Reason?.Trim()
        };

        await _repo.AddAsync(appointment);
    }

    public async Task UpdateAsync(int id, UpdateAppointmentRequest req)
    {
        var appointment = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Appointment with ID {id} not found");

        // Check patient exists
        _ = await _patientRepo.GetByIdAsync(req.PatientId)
            ?? throw new KeyNotFoundException($"Patient with ID {req.PatientId} not found");

        // Check doctor exists
        _ = await _staffRepo.GetByIdAsync(req.DoctorId)
            ?? throw new KeyNotFoundException($"Staff/Doctor with ID {req.DoctorId} not found");

        appointment.PatientId = req.PatientId;
        appointment.DoctorId = req.DoctorId;
        appointment.AppointmentDate = req.AppointmentDate;
        appointment.Type = string.IsNullOrWhiteSpace(req.Type) ? "First Visit" : req.Type.Trim();
        appointment.Status = string.IsNullOrWhiteSpace(req.Status) ? "Scheduled" : req.Status.Trim();
        appointment.Reason = req.Reason?.Trim();
        appointment.QueueNumber = req.QueueNumber;

        await _repo.UpdateAsync(appointment);
    }

    public async Task DeleteAsync(int id)
    {
        await _repo.DeleteAsync(id);
    }

    public async Task CheckInAsync(int id)
    {
        var appointment = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Appointment with ID {id} not found");

        if (appointment.Status != "Scheduled")
        {
            throw new InvalidOperationException($"Cannot check-in. Current appointment status is '{appointment.Status}' but must be 'Scheduled'.");
        }

        // Generate queue number for that doctor on the appointment's date
        var maxQueue = await _repo.GetMaxQueueNumberAsync(appointment.DoctorId, appointment.AppointmentDate);
        appointment.QueueNumber = maxQueue + 1;
        appointment.Status = "Waiting";

        await _repo.UpdateAsync(appointment);
    }

    private AppointmentDto MapToDto(Appointment a)
    {
        return new AppointmentDto(
            a.AppointmentId,
            a.PatientId,
            a.Patient != null ? $"{a.Patient.FirstName} {a.Patient.LastName}" : "Unknown Patient",
            a.DoctorId,
            a.Doctor != null ? $"{a.Doctor.FirstName} {a.Doctor.LastName}" : "Unknown Doctor",
            a.AppointmentDate,
            a.QueueNumber,
            a.Type,
            a.Status,
            a.Reason
        );
    }
}
