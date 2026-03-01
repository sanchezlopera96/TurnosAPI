using TurnosAPI.Application.DTOs.Request;
using TurnosAPI.Application.DTOs.Response;
using TurnosAPI.Application.Interfaces;
using TurnosAPI.Domain.Entities;
using TurnosAPI.Domain.Enums;
using TurnosAPI.Domain.Exceptions;
using TurnosAPI.Domain.Interfaces;

namespace TurnosAPI.Application.Services;

public class AppointmentService : IAppointmentService
{
    private const int MaxDailyAppointments = 5;
    private readonly IUnitOfWork _unitOfWork;

    public AppointmentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<ApiResponse<AppointmentResponse>> CreateAsync(
    CreateAppointmentRequest request,
    CancellationToken cancellationToken = default)
    {
        var activeAppointment = (await _unitOfWork.Appointments
            .GetByCustomerIdNumberAsync(request.CustomerIdNumber, cancellationToken))
            .FirstOrDefault(a =>
                a.Status == AppointmentStatus.Pending ||
                a.Status == AppointmentStatus.Active);

        if (activeAppointment != null)
            return ApiResponse<AppointmentResponse>.Fail(
                "Ya tienes un turno activo o pendiente. Espera a que expire o sea atendido antes de crear uno nuevo.");

        var todayCount = await _unitOfWork.Appointments
            .CountByCustomerTodayAsync(request.CustomerIdNumber, cancellationToken);

        if (todayCount >= MaxDailyAppointments)
            return ApiResponse<AppointmentResponse>.Fail(
                $"Has alcanzado el límite de {MaxDailyAppointments} turnos por día. Intenta nuevamente mañana.");

        var branch = await _unitOfWork.Branches.GetByIdAsync(request.BranchId, cancellationToken);
        if (branch is null || !branch.IsActive)
            return ApiResponse<AppointmentResponse>.Fail("La sucursal seleccionada no existe o no está activa.");

        var appointment = new Appointment(request.CustomerIdNumber, request.BranchId);
        await _unitOfWork.Appointments.AddAsync(appointment, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return ApiResponse<AppointmentResponse>.Ok(
            MapToResponse(appointment, branch.Name),
            "Turno agendado exitosamente. Tienes 15 minutos para llegar a la sucursal.");
    }

    public async Task<ApiResponse<AppointmentResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var appointment = await _unitOfWork.Appointments.GetByIdAsync(id, cancellationToken);
        if (appointment is null)
            return ApiResponse<AppointmentResponse>.Fail("Turno no encontrado.");

        var branch = await _unitOfWork.Branches.GetByIdAsync(appointment.BranchId, cancellationToken);
        return ApiResponse<AppointmentResponse>.Ok(MapToResponse(appointment, branch?.Name ?? "Unknown"));
    }

    public async Task<ApiResponse<IEnumerable<AppointmentResponse>>> GetByCustomerAsync(
        string idNumber,
        CancellationToken cancellationToken = default)
    {
        var appointments = await _unitOfWork.Appointments
            .GetByCustomerIdNumberAsync(idNumber, cancellationToken);

        var responses = new List<AppointmentResponse>();
        foreach (var appt in appointments)
        {
            var branch = await _unitOfWork.Branches.GetByIdAsync(appt.BranchId, cancellationToken);
            responses.Add(MapToResponse(appt, branch?.Name ?? "Unknown"));
        }

        return ApiResponse<IEnumerable<AppointmentResponse>>.Ok(responses);
    }

    public async Task<ApiResponse<IEnumerable<AppointmentResponse>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var appointments = await _unitOfWork.Appointments.GetAllAsync(cancellationToken);
        var responses = new List<AppointmentResponse>();

        foreach (var appt in appointments)
        {
            var branch = await _unitOfWork.Branches.GetByIdAsync(appt.BranchId, cancellationToken);
            responses.Add(MapToResponse(appt, branch?.Name ?? "Desconocida"));
        }

        return ApiResponse<IEnumerable<AppointmentResponse>>.Ok(responses);
    }

    public async Task<ApiResponse<AppointmentResponse>> ActivateAsync(
        Guid id,
        string customerIdNumber,
        CancellationToken cancellationToken = default)
    {
        var appointment = await _unitOfWork.Appointments.GetByIdAsync(id, cancellationToken);
        if (appointment is null)
            return ApiResponse<AppointmentResponse>.Fail("Turno no encontrado.");

        if (appointment.CustomerIdNumber != customerIdNumber)
            return ApiResponse<AppointmentResponse>.Fail("No estás autorizado para activar este turno.");

        try
        {
            appointment.Activate();
        }
        catch (DomainException ex)
        {
            return ApiResponse<AppointmentResponse>.Fail(ex.Message);
        }

        _unitOfWork.Appointments.Update(appointment);
        await _unitOfWork.CommitAsync(cancellationToken);

        var branch = await _unitOfWork.Branches.GetByIdAsync(appointment.BranchId, cancellationToken);
        return ApiResponse<AppointmentResponse>.Ok(
            MapToResponse(appointment, branch?.Name ?? "Desconocida"),
            "Turno activado exitosamente.");
    }

    public async Task<ApiResponse<AppointmentResponse>> UpdateStatusAsync(
        Guid id,
        UpdateAppointmentStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var appointment = await _unitOfWork.Appointments.GetByIdAsync(id, cancellationToken);
        if (appointment is null)
            return ApiResponse<AppointmentResponse>.Fail("Turno no encontrado.");

        try
        {
            switch (request.Status.ToLower())
            {
                case "attended":
                    appointment.MarkAsAttended();
                    break;
                case "cancelled":
                    appointment.Cancel();
                    break;
                default:
                    return ApiResponse<AppointmentResponse>.Fail("Estado inválido. Valores válidos: 'Attended', 'Cancelled'.");
            }
        }
        catch (DomainException ex)
        {
            return ApiResponse<AppointmentResponse>.Fail(ex.Message);
        }

        _unitOfWork.Appointments.Update(appointment);
        await _unitOfWork.CommitAsync(cancellationToken);

        var branch = await _unitOfWork.Branches.GetByIdAsync(appointment.BranchId, cancellationToken);
        return ApiResponse<AppointmentResponse>.Ok(MapToResponse(appointment, branch?.Name ?? "Desconocida"));
    }

    public async Task ProcessExpiredAppointmentsAsync(CancellationToken cancellationToken = default)
    {
        var expiredAppointments = await _unitOfWork.Appointments
            .GetExpiredPendingAsync(cancellationToken);

        foreach (var appointment in expiredAppointments)
        {
            appointment.Expire();
            _unitOfWork.Appointments.Update(appointment);
        }

        if (expiredAppointments.Any())
            await _unitOfWork.CommitAsync(cancellationToken);
    }

    private static AppointmentResponse MapToResponse(Appointment appointment, string branchName)
    {
        var remainingSeconds = appointment.Status == AppointmentStatus.Pending
            ? Math.Max(0, (int)(appointment.ExpiresAt - DateTime.UtcNow).TotalSeconds)
            : 0;

        return new AppointmentResponse(
            appointment.Id,
            appointment.Code,
            appointment.CustomerIdNumber,
            appointment.BranchId,
            branchName,
            appointment.Status.ToString(),
            appointment.CreatedAt,
            appointment.ExpiresAt,
            appointment.ActivatedAt,
            appointment.AttendedAt,
            remainingSeconds
        );
    }
}