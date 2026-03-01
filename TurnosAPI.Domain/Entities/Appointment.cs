using TurnosAPI.Domain.Enums;
using TurnosAPI.Domain.Exceptions;

namespace TurnosAPI.Domain.Entities;

public class Appointment
{
    private const int ExpirationMinutes = 15;

    public Guid Id { get; private set; }
    public string Code { get; private set; }
    public string CustomerIdNumber { get; private set; }
    public Guid BranchId { get; private set; }
    public Branch Branch { get; private set; }
    public AppointmentStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? ActivatedAt { get; private set; }
    public DateTime? AttendedAt { get; private set; }

    private Appointment() { }

    public Appointment(string customerIdNumber, Guid branchId)
    {
        if (string.IsNullOrWhiteSpace(customerIdNumber))
            throw new DomainException("Customer ID number is required.");

        if (branchId == Guid.Empty)
            throw new DomainException("A valid branch must be assigned.");

        Id = Guid.NewGuid();
        Code = GenerateCode();
        CustomerIdNumber = customerIdNumber;
        BranchId = branchId;
        Status = AppointmentStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        ExpiresAt = CreatedAt.AddMinutes(ExpirationMinutes);
    }

    public void Activate()
    {
        if (Status != AppointmentStatus.Pending)
            throw new DomainException($"Only pending appointments can be activated. Current status: {Status}.");

        if (DateTime.UtcNow > ExpiresAt)
            throw new DomainException("The appointment has expired. Please create a new one.");

        Status = AppointmentStatus.Active;
        ActivatedAt = DateTime.UtcNow;
    }

    public void Expire()
    {
        if (Status != AppointmentStatus.Pending)
            throw new DomainException("Only pending appointments can expire.");

        Status = AppointmentStatus.Expired;
    }

    public void MarkAsAttended()
    {
        if (Status != AppointmentStatus.Active)
            throw new DomainException("Only active appointments can be marked as attended.");

        Status = AppointmentStatus.Attended;
        AttendedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == AppointmentStatus.Attended || Status == AppointmentStatus.Expired)
            throw new DomainException("Cannot cancel an already attended or expired appointment.");

        Status = AppointmentStatus.Cancelled;
    }

    public bool IsWithinTimeWindow() => DateTime.UtcNow <= ExpiresAt;

    private static string GenerateCode()
    {
        var timestamp = DateTime.UtcNow.ToString("HHmmss");
        var random = new Random().Next(100, 999);
        return $"T{timestamp}{random}";
    }
}