using System.ComponentModel.DataAnnotations;

namespace TurnosAPI.Application.DTOs.Request;

public record CreateBranchRequest(
    [Required(ErrorMessage = "Branch name is required.")]
    string Name,

    [Required(ErrorMessage = "Address is required.")]
    string Address,

    [Required(ErrorMessage = "City is required.")]
    string City
);

public record UpdateBranchRequest(
    [Required] string Name,
    [Required] string Address,
    [Required] string City
);