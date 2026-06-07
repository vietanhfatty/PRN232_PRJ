using System;
using System.ComponentModel.DataAnnotations;

namespace MyProject.Application.DTOs;

public record AppointmentDto(
    int AppointmentId,
    int PatientId,
    string PatientName,
    int DoctorId,
    string DoctorName,
    DateTime AppointmentDate,
    int? QueueNumber,
    string? Type,
    string? Status,
    string? Reason
);

public record CreateAppointmentRequest(
    [Required(ErrorMessage = "Patient is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid patient.")]
    int PatientId,

    [Required(ErrorMessage = "Doctor is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid doctor.")]
    int DoctorId,

    [Required(ErrorMessage = "Appointment date is required.")]
    DateTime AppointmentDate,

    string? Type,
    string? Status,

    [StringLength(500, ErrorMessage = "Reason must not exceed 500 characters.")]
    string? Reason
);

public record UpdateAppointmentRequest(
    [Required(ErrorMessage = "Patient is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid patient.")]
    int PatientId,

    [Required(ErrorMessage = "Doctor is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid doctor.")]
    int DoctorId,

    [Required(ErrorMessage = "Appointment date is required.")]
    DateTime AppointmentDate,

    string? Type,
    string? Status,

    [StringLength(500, ErrorMessage = "Reason must not exceed 500 characters.")]
    string? Reason,

    [Range(1, 9999, ErrorMessage = "Queue number must be between 1 and 9999.")]
    int? QueueNumber
);
