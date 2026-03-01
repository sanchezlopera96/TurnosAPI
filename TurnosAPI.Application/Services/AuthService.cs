using TurnosAPI.Application.DTOs.Request;
using TurnosAPI.Application.DTOs.Response;
using TurnosAPI.Application.Interfaces;
using TurnosAPI.Domain.Interfaces;

namespace TurnosAPI.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(
        IUnitOfWork unitOfWork,
        IJwtTokenGenerator tokenGenerator,
        IPasswordHasher passwordHasher)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _tokenGenerator = tokenGenerator ?? throw new ArgumentNullException(nameof(tokenGenerator));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
    }

    public async Task<ApiResponse<AuthResponse>> AdminLoginAsync(
        AdminLoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.AdminUsers.GetByUsernameAsync(
            request.Username.Trim().ToLower(), cancellationToken);

        if (user is null || !user.IsActive)
            return ApiResponse<AuthResponse>.Fail("Invalid credentials.");

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            return ApiResponse<AuthResponse>.Fail("Invalid credentials.");

        var token = _tokenGenerator.GenerateToken(user.Username, "Admin", out var expiresAt);

        return ApiResponse<AuthResponse>.Ok(
            new AuthResponse(token, "Admin", user.Username, expiresAt),
            "Login successful.");
    }

    public Task<ApiResponse<AuthResponse>> ClientLoginAsync(
        ClientLoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var token = _tokenGenerator.GenerateToken(request.IdNumber, "Client", out var expiresAt);

        return Task.FromResult(ApiResponse<AuthResponse>.Ok(
            new AuthResponse(token, "Client", request.IdNumber, expiresAt),
            "Login successful."));
    }
}