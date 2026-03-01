using TurnosAPI.Application.DTOs.Request;
using TurnosAPI.Application.DTOs.Response;
using TurnosAPI.Application.Interfaces;
using TurnosAPI.Domain.Entities;
using TurnosAPI.Domain.Interfaces;

namespace TurnosAPI.Application.Services;

public class BranchService : IBranchService
{
    private readonly IUnitOfWork _unitOfWork;

    public BranchService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<ApiResponse<BranchResponse>> CreateAsync(
        CreateBranchRequest request,
        CancellationToken cancellationToken = default)
    {
        var branch = new Branch(request.Name, request.Address, request.City);
        await _unitOfWork.Branches.AddAsync(branch, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);
        return ApiResponse<BranchResponse>.Ok(MapToResponse(branch), "Branch created successfully.");
    }

    public async Task<ApiResponse<IEnumerable<BranchResponse>>> GetAllActiveAsync(
        CancellationToken cancellationToken = default)
    {
        var branches = await _unitOfWork.Branches.GetAllActiveAsync(cancellationToken);
        return ApiResponse<IEnumerable<BranchResponse>>.Ok(branches.Select(MapToResponse));
    }

    public async Task<ApiResponse<BranchResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var branch = await _unitOfWork.Branches.GetByIdAsync(id, cancellationToken);
        if (branch is null)
            return ApiResponse<BranchResponse>.Fail("Branch not found.");

        return ApiResponse<BranchResponse>.Ok(MapToResponse(branch));
    }

    public async Task<ApiResponse<BranchResponse>> UpdateAsync(
        Guid id,
        UpdateBranchRequest request,
        CancellationToken cancellationToken = default)
    {
        var branch = await _unitOfWork.Branches.GetByIdAsync(id, cancellationToken);
        if (branch is null)
            return ApiResponse<BranchResponse>.Fail("Branch not found.");

        branch.Update(request.Name, request.Address, request.City);
        _unitOfWork.Branches.Update(branch);
        await _unitOfWork.CommitAsync(cancellationToken);
        return ApiResponse<BranchResponse>.Ok(MapToResponse(branch), "Branch updated successfully.");
    }

    private static BranchResponse MapToResponse(Branch branch) =>
        new(branch.Id, branch.Name, branch.Address, branch.City, branch.IsActive);
}