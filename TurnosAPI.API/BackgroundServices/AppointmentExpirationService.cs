using TurnosAPI.Application.Interfaces;

namespace TurnosAPI.API.BackgroundServices;

public class AppointmentExpirationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AppointmentExpirationService> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

    public AppointmentExpirationService(
        IServiceProvider serviceProvider,
        ILogger<AppointmentExpirationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Appointment expiration service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(Interval, stoppingToken);

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var appointmentService = scope.ServiceProvider.GetRequiredService<IAppointmentService>();
                await appointmentService.ProcessExpiredAppointmentsAsync(stoppingToken);
                _logger.LogDebug("Expired appointments processed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing expired appointments.");
            }
        }
    }
}