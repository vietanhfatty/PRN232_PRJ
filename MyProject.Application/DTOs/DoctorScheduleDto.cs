using System;
using System.ComponentModel.DataAnnotations;

namespace MyProject.Application.DTOs;

public record DoctorScheduleDto(
    int ScheduleId,
    int DoctorId,
    string DoctorName,
    DateOnly WorkDate,
    string ShiftName,
    int? MaxPatients
);

public record CreateDoctorScheduleRequest(
    [Required(ErrorMessage = "Doctor is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid doctor.")]
    int DoctorId,

    [Required(ErrorMessage = "Work date is required.")]
    DateOnly WorkDate,

    [Required(ErrorMessage = "Shift name is required.")]
    string ShiftName,

    [Range(1, 200, ErrorMessage = "Max patients must be between 1 and 200.")]
    int? MaxPatients
);

public record UpdateDoctorScheduleRequest(
    [Required(ErrorMessage = "Doctor is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid doctor.")]
    int DoctorId,

    [Required(ErrorMessage = "Work date is required.")]
    DateOnly WorkDate,

    [Required(ErrorMessage = "Shift name is required.")]
    string ShiftName,

    [Range(1, 200, ErrorMessage = "Max patients must be between 1 and 200.")]
    int? MaxPatients
);
