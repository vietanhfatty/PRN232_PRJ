namespace MyProject.Application.DTOs;

public record LoginRequest(string Username, string Password);

public record LoginResponse(
    bool Success,
    string Message,
    int? StaffId,
    string? Username,
    string? RoleName,
    string? FullName
);
