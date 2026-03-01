using TurnosAPI.Domain.Entities;

namespace TurnosAPI.Domain.Interfaces;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Appointment>> GetByCustomerIdNumberAsync(string idNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<Appointment>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Appointment>> GetByBranchAsync(Guid branchId, CancellationToken cancellationToken = default);
    Task<int> CountByCustomerTodayAsync(string idNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<Appointment>> GetExpiredPendingAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Appointment appointment, CancellationToken cancellationToken = default);
    void Update(Appointment appointment);
}