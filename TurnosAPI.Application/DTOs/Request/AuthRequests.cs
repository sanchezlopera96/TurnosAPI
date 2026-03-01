using System.ComponentModel.DataAnnotations;

namespace TurnosAPI.Application.DTOs.Request;

public record AdminLoginRequest(
    [Required(ErrorMessage = "Username is required.")]
    string Username,

    [Required(ErrorMessage = "Password is required.")]
    string Password
);

public record ClientLoginRequest(
    [Required(ErrorMessage = "Customer ID number is required.")]
    [StringLength(20, MinimumLength = 5, ErrorMessage = "ID number must be between 5 and 20 characters.")]
    string IdNumber
);