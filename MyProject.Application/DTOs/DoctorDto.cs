using System.ComponentModel.DataAnnotations;

namespace MyProject.Application.DTOs;

public record DoctorDto(
    int DoctorId,
    int UserId,
    string FullName,
    string Username,
    string? Phone,
    string Specialization,
    int ExperienceYears,
    string? Description
);

public record CreateDoctorRequest(
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

    [Required(ErrorMessage = "Specialization is required.")]
    [StringLength(100, ErrorMessage = "Specialization must not exceed 100 characters.")]
    string Specialization,

    [Range(0, 100, ErrorMessage = "Experience years must be between 0 and 100.")]
    int ExperienceYears,

    [StringLength(1000, ErrorMessage = "Description must not exceed 1000 characters.")]
    string? Description
);

public record UpdateDoctorRequest(
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Full name must be 1-100 characters.")]
    string FullName,

    string? Phone,

    [Required(ErrorMessage = "Specialization is required.")]
    [StringLength(100, ErrorMessage = "Specialization must not exceed 100 characters.")]
    string Specialization,

    [Range(0, 100, ErrorMessage = "Experience years must be between 0 and 100.")]
    int ExperienceYears,

    [StringLength(1000, ErrorMessage = "Description must not exceed 1000 characters.")]
    string? Description
);
