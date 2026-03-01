using TurnosAPI.Domain.Entities;
using TurnosAPI.Domain.Enums;
using TurnosAPI.Domain.Exceptions;
using Xunit;

namespace TurnosAPI.Tests.Unit;

public class AppointmentEntityTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreatePendingAppointment()
    {
        // Arrange & Act
        var appointment = new Appointment("123456789", Guid.NewGuid());

        // Assert
        Assert.Equal(AppointmentStatus.Pending, appointment.Status);
        Assert.NotEmpty(appointment.Code);
        Assert.True(appointment.ExpiresAt > DateTime.UtcNow);
        Assert.True(appointment.IsWithinTimeWindow());
    }

    [Fact]
    public void Constructor_WithEmptyIdNumber_ShouldThrowDomainException()
    {
        Assert.Throws<DomainException>(() => new Appointment("", Guid.NewGuid()));
        Assert.Throws<DomainException>(() => new Appointment("   ", Guid.NewGuid()));
    }

    [Fact]
    public void Constructor_WithEmptyBranchId_ShouldThrowDomainException()
    {
        Assert.Throws<DomainException>(() => new Appointment("123456789", Guid.Empty));
    }

    [Fact]
    public void Activate_WhenPendingAndWithinTimeWindow_ShouldSetActiveStatus()
    {
        // Arrange
        var appointment = new Appointment("123456789", Guid.NewGuid());

        // Act
        appointment.Activate();

        // Assert
        Assert.Equal(AppointmentStatus.Active, appointment.Status);
        Assert.NotNull(appointment.ActivatedAt);
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ShouldThrowDomainException()
    {
        // Arrange
        var appointment = new Appointment("123456789", Guid.NewGuid());
        appointment.Activate();

        // Act & Assert
        Assert.Throws<DomainException>(() => appointment.Activate());
    }

    [Fact]
    public void MarkAsAttended_WhenActive_ShouldSetAttendedStatus()
    {
        // Arrange
        var appointment = new Appointment("123456789", Guid.NewGuid());
        appointment.Activate();

        // Act
        appointment.MarkAsAttended();

        // Assert
        Assert.Equal(AppointmentStatus.Attended, appointment.Status);
        Assert.NotNull(appointment.AttendedAt);
    }

    [Fact]
    public void MarkAsAttended_WhenPending_ShouldThrowDomainException()
    {
        var appointment = new Appointment("123456789", Guid.NewGuid());
        Assert.Throws<DomainException>(() => appointment.MarkAsAttended());
    }

    [Fact]
    public void Cancel_WhenPending_ShouldSetCancelledStatus()
    {
        var appointment = new Appointment("123456789", Guid.NewGuid());
        appointment.Cancel();
        Assert.Equal(AppointmentStatus.Cancelled, appointment.Status);
    }

    [Fact]
    public void Cancel_WhenAlreadyAttended_ShouldThrowDomainException()
    {
        var appointment = new Appointment("123456789", Guid.NewGuid());
        appointment.Activate();
        appointment.MarkAsAttended();
        Assert.Throws<DomainException>(() => appointment.Cancel());
    }

    [Fact]
    public void Expire_WhenPending_ShouldSetExpiredStatus()
    {
        var appointment = new Appointment("123456789", Guid.NewGuid());
        appointment.Expire();
        Assert.Equal(AppointmentStatus.Expired, appointment.Status);
    }

    [Fact]
    public void Expire_WhenActive_ShouldThrowDomainException()
    {
        var appointment = new Appointment("123456789", Guid.NewGuid());
        appointment.Activate();
        Assert.Throws<DomainException>(() => appointment.Expire());
    }
}