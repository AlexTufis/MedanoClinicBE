using Hangfire;
using MedanoClinicBE.Services.Interfaces;

namespace MedanoClinicBE.Services
{
    public class HangfireJobSetupService : IHostedService
    {
        private readonly ILogger<HangfireJobSetupService> _logger;

        public HangfireJobSetupService(ILogger<HangfireJobSetupService> logger)
        {
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Setting up recurring jobs...");

                // Add recurring job to update past appointments status every hour
                RecurringJob.AddOrUpdate<IJobService>(
                    "update-past-appointments-status",
                    x => x.ProcessPastAppointmentsStatusUpdateAsync(),
                    Cron.Hourly, // Runs every hour
                    TimeZoneInfo.Local);

                _logger.LogInformation("Recurring job 'update-past-appointments-status' scheduled to run every hour");

                // Optional: Add more recurring jobs here in the future
                // RecurringJob.AddOrUpdate<IJobService>(
                //     "cleanup-old-notifications",
                //     x => x.CleanupOldNotificationsAsync(),
                //     Cron.Daily,
                //     TimeZoneInfo.Local);

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while setting up recurring jobs");
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping Hangfire job setup service");
            return Task.CompletedTask;
        }
    }
}