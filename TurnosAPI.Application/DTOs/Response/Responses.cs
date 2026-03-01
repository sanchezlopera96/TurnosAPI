namespace TurnosAPI.Application.DTOs.Response;

public record AppointmentResponse(
    Guid Id,
    string Code,
    string CustomerIdNumber,
    Guid BranchId,
    string BranchName,
    string Status,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    DateTime? ActivatedAt,
    DateTime? AttendedAt,
    int RemainingSeconds
);

public record BranchResponse(
    Guid Id,
    string Name,
    string Address,
    string City,
    bool IsActive
);

public record AuthResponse(
    string Token,
    string Role,
    string Identifier,
    DateTime ExpiresAt
);