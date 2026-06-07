using System.ComponentModel.DataAnnotations;

namespace MyProject.Application.DTOs;

public record LabTestDto(
    int TestId,
    string TestName,
    decimal Cost
);

public record CreateLabTestRequest(
    [Required(ErrorMessage = "Test name is required.")]
    [StringLength(150, MinimumLength = 1, ErrorMessage = "Test name must be 1–150 characters.")]
    string TestName,

    [Required(ErrorMessage = "Cost is required.")]
    [Range(0, 999999999, ErrorMessage = "Cost must be 0 or greater.")]
    decimal Cost
);

public record UpdateLabTestRequest(
    [Required(ErrorMessage = "Test name is required.")]
    [StringLength(150, MinimumLength = 1, ErrorMessage = "Test name must be 1–150 characters.")]
    string TestName,

    [Required(ErrorMessage = "Cost is required.")]
    [Range(0, 999999999, ErrorMessage = "Cost must be 0 or greater.")]
    decimal Cost
);
