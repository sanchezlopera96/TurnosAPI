using TurnosAPI.Domain.Interfaces;
using TurnosAPI.Infrastructure.Persistence.Repositories;

namespace TurnosAPI.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public IAppointmentRepository Appointments { get; }
    public IBranchRepository Branches { get; }
    public IAdminUserRepository AdminUsers { get; }

    public UnitOfWork(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        Appointments = new AppointmentRepository(context);
        Branches = new BranchRepository(context);
        AdminUsers = new AdminUserRepository(context);
    }

    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);

    public void Dispose() => _context.Dispose();
}