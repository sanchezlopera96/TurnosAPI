using TurnosAPI.Domain.Entities;

namespace TurnosAPI.Domain.Interfaces;

public interface IBranchRepository
{
    Task<Branch?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Branch>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Branch branch, CancellationToken cancellationToken = default);
    void Update(Branch branch);
}