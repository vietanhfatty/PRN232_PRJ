using System.ComponentModel.DataAnnotations;

namespace MyProject.Application.DTOs;

public record StaffDto(
    int StaffId,
    string FirstName,
    string LastName,
    string? Specialization,
    string Phone,
    string? Email,
    string? Status,
    int? AccountId,
    string? Username,
    int? RoleId,
    string? RoleName
);

public record CreateStaffRequest(
    [Required(ErrorMessage = "First name is required.")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "First name must be 1–50 characters.")]
    string FirstName,

    [Required(ErrorMessage = "Last name is required.")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Last name must be 1–50 characters.")]
    string LastName,

    [StringLength(100, ErrorMessage = "Specialization must not exceed 100 characters.")]
    string? Specialization,

    [Required(ErrorMessage = "Phone number is required.")]
    [RegularExpression(@"^[0-9+\-\s]{7,20}$", ErrorMessage = "Phone must be 7–20 digits/characters.")]
    string Phone,

    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    [StringLength(100, ErrorMessage = "Email must not exceed 100 characters.")]
    string? Email,

    string? Status,

    [StringLength(50, ErrorMessage = "Username must not exceed 50 characters.")]
    string? Username,

    [StringLength(100, ErrorMessage = "Password must not exceed 100 characters.")]
    string? Password,

    int? RoleId
);

public record UpdateStaffRequest(
    [Required(ErrorMessage = "First name is required.")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "First name must be 1–50 characters.")]
    string FirstName,

    [Required(ErrorMessage = "Last name is required.")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Last name must be 1–50 characters.")]
    string LastName,

    [StringLength(100, ErrorMessage = "Specialization must not exceed 100 characters.")]
    string? Specialization,

    [Required(ErrorMessage = "Phone number is required.")]
    [RegularExpression(@"^[0-9+\-\s]{7,20}$", ErrorMessage = "Phone must be 7–20 digits/characters.")]
    string Phone,

    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    [StringLength(100, ErrorMessage = "Email must not exceed 100 characters.")]
    string? Email,

    string? Status,

    [StringLength(50, ErrorMessage = "Username must not exceed 50 characters.")]
    string? Username,

    // Password is intentionally not [Required] — leave blank to keep existing hash
    [StringLength(100, ErrorMessage = "Password must not exceed 100 characters.")]
    string? Password,

    int? RoleId
);
