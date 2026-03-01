using TurnosAPI.Domain.Enums;
using TurnosAPI.Domain.Exceptions;

namespace TurnosAPI.Domain.Entities;

public class AdminUser
{
    public Guid Id { get; private set; }
    public string Username { get; private set; }
    public string PasswordHash { get; private set; }
    public string FullName { get; private set; }
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private AdminUser() { }

    public AdminUser(string username, string passwordHash, string fullName)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new DomainException("Username is required.");

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Password hash is required.");

        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException("Full name is required.");

        Id = Guid.NewGuid();
        Username = username.Trim().ToLower();
        PasswordHash = passwordHash;
        FullName = fullName.Trim();
        Role = UserRole.Admin;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdatePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new DomainException("Password hash is required.");

        PasswordHash = newPasswordHash;
    }

    public void Deactivate() => IsActive = false;
}