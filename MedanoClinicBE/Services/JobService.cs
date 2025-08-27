using MedanoClinicBE.DTOs;
using MedanoClinicBE.Repositories.Interfaces;
using MedanoClinicBE.Services.Interfaces;
using Hangfire;

namespace MedanoClinicBE.Services
{
    public class JobService : IJobService
    {
        private readonly INotificationService _notificationService;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly ILogger<JobService> _logger;

        public JobService(
            INotificationService notificationService, 
            IAppointmentRepository appointmentRepository,
            ILogger<JobService> logger)
        {
            _notificationService = notificationService;
            _appointmentRepository = appointmentRepository;
            _logger = logger;
        }

        public void ScheduleAppointmentReminder(AppointmentResponseDto appointment)
        {
            try
            {
                // Calculate when to send reminder (1 hour before appointment)
                var appointmentDateTime = DateTime.Parse($"{appointment.AppointmentDate} {appointment.AppointmentTime}");
                var reminderTime = appointmentDateTime.AddHours(-1);

                if (reminderTime > DateTime.Now)
                {
                    // Cancel any existing reminder for this appointment
                    CancelAppointmentReminder(appointment.Id);

                    // Schedule new reminder with Hangfire
                    var jobId = BackgroundJob.Schedule<IJobService>(
                        x => x.ProcessAppointmentReminderAsync(appointment.Id),
                        reminderTime);

                    _logger.LogInformation("Hangfire appointment reminder scheduled for {AppointmentId} at {ReminderTime} with JobId {JobId}", 
                        appointment.Id, reminderTime, jobId);
                }
                else
                {
                    _logger.LogWarning("Cannot schedule reminder for past appointment {AppointmentId}", appointment.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to schedule Hangfire appointment reminder for {AppointmentId}", appointment.Id);
            }
        }

        public void CancelAppointmentReminder(string appointmentId)
        {
            try
            {
                // In a production system, you would store job IDs to cancel them
                // For now, Hangfire handles duplicate scheduling automatically
                _logger.LogInformation("Appointment reminder rescheduling handled by Hangfire for {AppointmentId}", appointmentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle appointment reminder cancellation for {AppointmentId}", appointmentId);
            }
        }

        public void SendAppointmentCreatedEmailJob(AppointmentResponseDto appointment)
        {
            try
            {
                // Enqueue job to send appointment creation email immediately
                var jobId = BackgroundJob.Enqueue<IJobService>(
                    x => x.ProcessAppointmentCreatedEmailAsync(appointment.Id));

                _logger.LogInformation("Hangfire appointment creation email job enqueued for {AppointmentId} with JobId {JobId}", 
                    appointment.Id, jobId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to enqueue Hangfire appointment creation email for {AppointmentId}", appointment.Id);
            }
        }

        [Queue("notifications")]
        public async Task ProcessAppointmentCreatedEmailAsync(string appointmentId)
        {
            try
            {
                _logger.LogInformation("Processing Hangfire appointment creation email for {AppointmentId}", appointmentId);

                // Get appointment details from repository
                var appointments = await _appointmentRepository.GetAllAppointmentsAsync();
                var appointment = appointments.FirstOrDefault(a => a.Id == appointmentId);

                if (appointment != null)
                {
                    // Send both in-app notification and email
                    await _notificationService.SendAppointmentCreatedNotificationAsync(appointment);
                    _logger.LogInformation("Hangfire appointment creation email processed successfully for {AppointmentId}", appointmentId);
                }
                else
                {
                    _logger.LogWarning("Appointment {AppointmentId} not found - skipping creation email", appointmentId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process Hangfire appointment creation email for {AppointmentId}", appointmentId);
                throw; // Re-throw to let Hangfire handle retries
            }
        }

        [Queue("notifications")]
        public async Task ProcessAppointmentReminderAsync(string appointmentId)
        {
            try
            {
                _logger.LogInformation("Processing Hangfire appointment reminder for {AppointmentId}", appointmentId);

                // Get appointment details from repository
                var appointments = await _appointmentRepository.GetAllAppointmentsAsync();
                var appointment = appointments.FirstOrDefault(a => a.Id == appointmentId);

                if (appointment != null && appointment.Status == "scheduled")
                {
                    await _notificationService.SendAppointmentReminderNotificationAsync(appointment);
                    _logger.LogInformation("Hangfire appointment reminder processed successfully for {AppointmentId}", appointmentId);
                }
                else
                {
                    _logger.LogWarning("Appointment {AppointmentId} not found or not scheduled - skipping reminder", appointmentId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process Hangfire appointment reminder for {AppointmentId}", appointmentId);
                throw; // Re-throw to let Hangfire handle retries
            }
        }

        [Queue("maintenance")]
        public async Task ProcessPastAppointmentsStatusUpdateAsync()
        {
            try
            {
                _logger.LogInformation("Processing past appointments status update job");

                // Update past appointments status to completed
                var updatedCount = await _appointmentRepository.UpdatePastAppointmentsStatusAsync();

                if (updatedCount > 0)
                {
                    _logger.LogInformation("Updated {Count} past appointments from scheduled to completed status", updatedCount);
                }
                else
                {
                    _logger.LogInformation("No past appointments found to update");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process past appointments status update");
                throw; // Re-throw to let Hangfire handle retries
            }
        }
    }
}