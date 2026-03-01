namespace TurnosAPI.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(string identifier, string role, out DateTime expiresAt);
}