using Moq;
using TurnosAPI.Application.DTOs.Request;
using TurnosAPI.Application.Services;
using TurnosAPI.Domain.Entities;
using TurnosAPI.Domain.Interfaces;
using Xunit;

namespace TurnosAPI.Tests.Unit.Services;

public class AppointmentServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IAppointmentRepository> _appointmentRepoMock;
    private readonly Mock<IBranchRepository> _branchRepoMock;
    private readonly AppointmentService _sut;

    public AppointmentServiceTests()
    {
        _appointmentRepoMock = new Mock<IAppointmentRepository>();
        _branchRepoMock = new Mock<IBranchRepository>();

        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _unitOfWorkMock.Setup(u => u.Appointments).Returns(_appointmentRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Branches).Returns(_branchRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _sut = new AppointmentService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task CreateAsync_WhenValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var branchId = Guid.NewGuid();
        var branch = new Branch("Main Branch", "123 Main St", "Bogota");
        var request = new CreateAppointmentRequest("123456789", branchId);

        _appointmentRepoMock
            .Setup(r => r.CountByCustomerTodayAsync("123456789", It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        _branchRepoMock
            .Setup(r => r.GetByIdAsync(branchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(branch);

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("123456789", result.Data.CustomerIdNumber);
        _appointmentRepoMock.Verify(r => r.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenDailyLimitReached_ShouldReturnFailure()
    {
        // Arrange
        var request = new CreateAppointmentRequest("123456789", Guid.NewGuid());

        _appointmentRepoMock
            .Setup(r => r.CountByCustomerTodayAsync("123456789", It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("daily limit", result.Message, StringComparison.OrdinalIgnoreCase);
        _appointmentRepoMock.Verify(r => r.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenBranchNotFound_ShouldReturnFailure()
    {
        // Arrange
        var branchId = Guid.NewGuid();
        var request = new CreateAppointmentRequest("123456789", branchId);

        _appointmentRepoMock
            .Setup(r => r.CountByCustomerTodayAsync("123456789", It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        _branchRepoMock
            .Setup(r => r.GetByIdAsync(branchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Branch?)null);

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("branch", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ActivateAsync_WhenAppointmentNotBelongsToCustomer_ShouldReturnFailure()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var appointment = new Appointment("999999999", Guid.NewGuid());

        _appointmentRepoMock
            .Setup(r => r.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        // Act
        var result = await _sut.ActivateAsync(appointmentId, "123456789");

        // Assert
        Assert.False(result.Success);
        Assert.Contains("not authorized", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ShouldReturnFailure()
    {
        // Arrange
        var id = Guid.NewGuid();
        _appointmentRepoMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        // Act
        var result = await _sut.GetByIdAsync(id);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("not found", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateStatusAsync_WhenInvalidStatus_ShouldReturnFailure()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var appointment = new Appointment("123456789", Guid.NewGuid());
        appointment.Activate();

        _appointmentRepoMock
            .Setup(r => r.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        var request = new UpdateAppointmentStatusRequest("InvalidStatus");

        // Act
        var result = await _sut.UpdateStatusAsync(appointmentId, request);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Invalid status", result.Message, StringComparison.OrdinalIgnoreCase);
    }
}