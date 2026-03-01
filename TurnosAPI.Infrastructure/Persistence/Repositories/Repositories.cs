using Microsoft.EntityFrameworkCore;
using TurnosAPI.Domain.Entities;
using TurnosAPI.Domain.Enums;
using TurnosAPI.Domain.Interfaces;

namespace TurnosAPI.Infrastructure.Persistence.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly AppDbContext _context;

    public AppointmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Appointments
            .Include(a => a.Branch)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<IEnumerable<Appointment>> GetByCustomerIdNumberAsync(
        string idNumber, CancellationToken cancellationToken = default)
        => await _context.Appointments
            .Include(a => a.Branch)
            .Where(a => a.CustomerIdNumber == idNumber)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<Appointment>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Appointments
            .Include(a => a.Branch)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<Appointment>> GetByBranchAsync(
        Guid branchId, CancellationToken cancellationToken = default)
        => await _context.Appointments
            .Where(a => a.BranchId == branchId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<int> CountByCustomerTodayAsync(
        string idNumber, CancellationToken cancellationToken = default)
    {
        var todayUtc = DateTime.UtcNow.Date;
        return await _context.Appointments
            .CountAsync(a =>
                a.CustomerIdNumber == idNumber &&
                a.CreatedAt.Date == todayUtc,
                cancellationToken);
    }

    public async Task<IEnumerable<Appointment>> GetExpiredPendingAsync(
        CancellationToken cancellationToken = default)
        => await _context.Appointments
            .Where(a =>
                a.Status == AppointmentStatus.Pending &&
                a.ExpiresAt < DateTime.UtcNow)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Appointment appointment, CancellationToken cancellationToken = default)
        => await _context.Appointments.AddAsync(appointment, cancellationToken);

    public void Update(Appointment appointment)
        => _context.Appointments.Update(appointment);
}

public class BranchRepository : IBranchRepository
{
    private readonly AppDbContext _context;

    public BranchRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Branch?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Branches.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public async Task<IEnumerable<Branch>> GetAllActiveAsync(CancellationToken cancellationToken = default)
        => await _context.Branches
            .Where(b => b.IsActive)
            .OrderBy(b => b.Name)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Branch branch, CancellationToken cancellationToken = default)
        => await _context.Branches.AddAsync(branch, cancellationToken);

    public void Update(Branch branch)
        => _context.Branches.Update(branch);
}

public class AdminUserRepository : IAdminUserRepository
{
    private readonly AppDbContext _context;

    public AdminUserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AdminUser?> GetByUsernameAsync(
        string username, CancellationToken cancellationToken = default)
        => await _context.AdminUsers
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);

    public async Task<AdminUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.AdminUsers.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task AddAsync(AdminUser user, CancellationToken cancellationToken = default)
        => await _context.AdminUsers.AddAsync(user, cancellationToken);
}