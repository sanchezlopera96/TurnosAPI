using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurnosAPI.Application.DTOs.Request;
using TurnosAPI.Application.Interfaces;

namespace TurnosAPI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService ?? throw new ArgumentNullException(nameof(appointmentService));
    }

    [HttpPost]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> Create(
        [FromBody] CreateAppointmentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _appointmentService.CreateAsync(request, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? branchId,
        [FromQuery] string? status,
        [FromQuery] bool todayOnly = true,
        CancellationToken cancellationToken = default)
    {
        var result = await _appointmentService.GetAllAsync(branchId, status, todayOnly, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _appointmentService.GetByIdAsync(id, cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("my-appointments")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> GetMyAppointments(CancellationToken cancellationToken)
    {
        var idNumber = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _appointmentService.GetByCustomerAsync(idNumber, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}/activate")]
    [Authorize]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        var isAdmin = User.IsInRole("Admin");
        var idNumber = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? "";

        var result = await _appointmentService.ActivateAsync(id, idNumber, isAdmin, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id:guid}/status")]
    [Authorize]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromBody] UpdateAppointmentStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (User.IsInRole("Client") && request.Status.ToLower() != "cancelled")
            return Forbid();

        var result = await _appointmentService.UpdateStatusAsync(id, request, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}