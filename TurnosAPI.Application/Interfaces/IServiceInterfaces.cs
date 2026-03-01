using TurnosAPI.Application.DTOs.Request;
using TurnosAPI.Application.DTOs.Response;

namespace TurnosAPI.Application.Interfaces;

public interface IAppointmentService
{
    Task<ApiResponse<AppointmentResponse>> CreateAsync(CreateAppointmentRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<AppointmentResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<IEnumerable<AppointmentResponse>>> GetByCustomerAsync(string idNumber, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<AppointmentResponse>>> GetAllAsync(Guid? branchId = null, string? status = null, bool todayOnly = true, CancellationToken cancellationToken = default);
    Task<ApiResponse<AppointmentResponse>> ActivateAsync(Guid id,string customerIdNumber,bool isAdmin = false, CancellationToken cancellationToken = default);
    Task<ApiResponse<AppointmentResponse>> UpdateStatusAsync(Guid id, UpdateAppointmentStatusRequest request, CancellationToken cancellationToken = default);
    Task ProcessExpiredAppointmentsAsync(CancellationToken cancellationToken = default);
}

public interface IBranchService
{
    Task<ApiResponse<BranchResponse>> CreateAsync(CreateBranchRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<IEnumerable<BranchResponse>>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<BranchResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<BranchResponse>> UpdateAsync(Guid id, UpdateBranchRequest request, CancellationToken cancellationToken = default);
}

public interface IAuthService
{
    Task<ApiResponse<AuthResponse>> AdminLoginAsync(AdminLoginRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<AuthResponse>> ClientLoginAsync(ClientLoginRequest request, CancellationToken cancellationToken = default);
}