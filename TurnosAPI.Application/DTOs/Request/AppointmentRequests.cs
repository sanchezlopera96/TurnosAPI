using System.ComponentModel.DataAnnotations;

namespace TurnosAPI.Application.DTOs.Request;

public record CreateAppointmentRequest(
    [Required(ErrorMessage = "Customer ID number is required.")]
    [StringLength(20, MinimumLength = 5, ErrorMessage = "ID number must be between 5 and 20 characters.")]
    string CustomerIdNumber,

    [Required(ErrorMessage = "Branch ID is required.")]
    Guid BranchId
);

public record UpdateAppointmentStatusRequest(
    [Required(ErrorMessage = "Status is required.")]
    string Status
);