using System.ComponentModel.DataAnnotations;

namespace MyProject.Application.DTOs;

public record MedicineDto(
    int MedicineId,
    string Name,
    string Unit,
    decimal Price,
    int StockQuantity
);

public record CreateMedicineRequest(
    [Required(ErrorMessage = "Medicine name is required.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be 1–100 characters.")]
    string Name,

    [Required(ErrorMessage = "Unit is required.")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Unit must be 1–50 characters.")]
    string Unit,

    [Required(ErrorMessage = "Price is required.")]
    [Range(0, 999999999, ErrorMessage = "Price must be 0 or greater.")]
    decimal Price,

    [Required(ErrorMessage = "Stock quantity is required.")]
    [Range(0, 999999, ErrorMessage = "Stock quantity must be 0 or greater.")]
    int StockQuantity
);

public record UpdateMedicineRequest(
    [Required(ErrorMessage = "Medicine name is required.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be 1–100 characters.")]
    string Name,

    [Required(ErrorMessage = "Unit is required.")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Unit must be 1–50 characters.")]
    string Unit,

    [Required(ErrorMessage = "Price is required.")]
    [Range(0, 999999999, ErrorMessage = "Price must be 0 or greater.")]
    decimal Price,

    [Required(ErrorMessage = "Stock quantity is required.")]
    [Range(0, 999999, ErrorMessage = "Stock quantity must be 0 or greater.")]
    int StockQuantity
);
