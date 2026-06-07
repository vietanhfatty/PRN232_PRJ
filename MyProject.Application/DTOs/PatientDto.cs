using System;
using System.ComponentModel.DataAnnotations;

namespace MyProject.Application.DTOs;

public record PatientDto(
    int PatientId,
    string FirstName,
    string LastName,
    string Gender,
    DateOnly Dob,
    string Phone,
    string? Address,
    string? InsuranceNo
);

public record CreatePatientRequest(
    [Required(ErrorMessage = "First name is required.")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "First name must be 1–50 characters.")]
    string FirstName,

    [Required(ErrorMessage = "Last name is required.")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Last name must be 1–50 characters.")]
    string LastName,

    [Required(ErrorMessage = "Gender is required.")]
    string Gender,

    [Required(ErrorMessage = "Date of birth is required.")]
    DateOnly Dob,

    [Required(ErrorMessage = "Phone number is required.")]
    [RegularExpression(@"^[0-9+\-\s]{7,20}$", ErrorMessage = "Phone must be 7–20 digits/characters.")]
    string Phone,

    [StringLength(200, ErrorMessage = "Address must not exceed 200 characters.")]
    string? Address,

    [StringLength(20, ErrorMessage = "Insurance number must not exceed 20 characters.")]
    string? InsuranceNo
);

public record UpdatePatientRequest(
    [Required(ErrorMessage = "First name is required.")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "First name must be 1–50 characters.")]
    string FirstName,

    [Required(ErrorMessage = "Last name is required.")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Last name must be 1–50 characters.")]
    string LastName,

    [Required(ErrorMessage = "Gender is required.")]
    string Gender,

    [Required(ErrorMessage = "Date of birth is required.")]
    DateOnly Dob,

    [Required(ErrorMessage = "Phone number is required.")]
    [RegularExpression(@"^[0-9+\-\s]{7,20}$", ErrorMessage = "Phone must be 7–20 digits/characters.")]
    string Phone,

    [StringLength(200, ErrorMessage = "Address must not exceed 200 characters.")]
    string? Address,

    [StringLength(20, ErrorMessage = "Insurance number must not exceed 20 characters.")]
    string? InsuranceNo
);
