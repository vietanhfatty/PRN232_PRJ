using System;
using System.ComponentModel.DataAnnotations;

namespace MyProject.Application.DTOs;

public record PatientDto(
    int PatientId,
    int UserId,
    string FullName,
    string Username,
    string? Phone,
    DateOnly? DateOfBirth,
    string? Gender,
    string? Address,
    string? BloodType,
    string? EmergencyContactName,
    string? EmergencyContactPhone
);

public record CreatePatientRequest(
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Full name must be 1-100 characters.")]
    string FullName,

    [Required(ErrorMessage = "Username is required.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Username must be 1-100 characters.")]
    string Username,

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(255, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
    string Password,

    string? Phone,
    DateOnly? DateOfBirth,
    string? Gender,
    string? Address,
    string? BloodType,
    string? EmergencyContactName,
    string? EmergencyContactPhone
);

public record UpdatePatientRequest(
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Full name must be 1-100 characters.")]
    string FullName,

    string? Phone,
    DateOnly? DateOfBirth,
    string? Gender,
    string? Address,
    string? BloodType,
    string? EmergencyContactName,
    string? EmergencyContactPhone
);
