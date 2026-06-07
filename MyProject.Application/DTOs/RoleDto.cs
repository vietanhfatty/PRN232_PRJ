using System.ComponentModel.DataAnnotations;

namespace MyProject.Application.DTOs;

public record RoleDto(int RoleId, string RoleName);

public record CreateRoleRequest(
    [Required(ErrorMessage = "Role name is required.")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Role name must be 1–50 characters.")]
    string RoleName
);

public record UpdateRoleRequest(
    [Required(ErrorMessage = "Role name is required.")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Role name must be 1–50 characters.")]
    string RoleName
);
