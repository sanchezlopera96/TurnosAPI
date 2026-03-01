namespace TurnosAPI.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IAppointmentRepository Appointments { get; }
    IBranchRepository Branches { get; }
    IAdminUserRepository AdminUsers { get; }

    Task<int> CommitAsync(CancellationToken cancellationToken = default);
}