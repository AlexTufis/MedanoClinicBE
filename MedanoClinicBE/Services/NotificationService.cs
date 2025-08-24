using MedanoClinicBE.DTOs;
using MedanoClinicBE.Services.Interfaces;

namespace MedanoClinicBE.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<NotificationService> _logger;
        // In-memory storage for now - in production, use database
        private static readonly List<NotificationDto> _notifications = new();

        public NotificationService(IEmailService emailService, ILogger<NotificationService> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<NotificationDto> CreateNotificationAsync(NotificationDto notification)
        {
            try
            {
                _notifications.Add(notification);
                _logger.LogInformation("Notification created for user {UserId} with type {Type}", 
                    notification.UserId, notification.Type);
                return await Task.FromResult(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create notification for user {UserId}", notification.UserId);
                throw;
            }
        }

        public async Task<List<NotificationDto>> GetUserNotificationsAsync(string userId)
        {
            try
            {
                var userNotifications = _notifications
                    .Where(n => n.UserId == userId)
                    .OrderByDescending(n => n.CreatedAt)
                    .ToList();

                return await Task.FromResult(userNotifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get notifications for user {UserId}", userId);
                return new List<NotificationDto>();
            }
        }

        public async Task<bool> MarkNotificationAsReadAsync(string notificationId)
        {
            try
            {
                var notification = _notifications.FirstOrDefault(n => n.Id == notificationId);
                if (notification != null)
                {
                    notification.IsRead = true;
                    _logger.LogInformation("Notification {NotificationId} marked as read", notificationId);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to mark notification {NotificationId} as read", notificationId);
                return false;
            }
        }

        public async Task<bool> SendAppointmentCreatedNotificationAsync(AppointmentResponseDto appointment)
        {
            try
            {
                // Create in-app notification
                var notification = new NotificationDto
                {
                    UserId = appointment.ClientId,
                    Title = "Appointment Confirmed",
                    Message = $"Your appointment with {appointment.DoctorName} on {appointment.AppointmentDate} at {appointment.AppointmentTime} has been confirmed.",
                    Type = NotificationType.AppointmentCreated,
                    AppointmentId = appointment.Id
                };

                await CreateNotificationAsync(notification);

                // Send email notification
                var emailSent = await _emailService.SendAppointmentCreatedEmailAsync(appointment);
                notification.EmailSent = emailSent;

                _logger.LogInformation("Appointment created notifications sent for appointment {AppointmentId}", appointment.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send appointment created notifications for appointment {AppointmentId}", appointment.Id);
                return false;
            }
        }

        public async Task<bool> SendAppointmentModifiedNotificationAsync(AppointmentResponseDto appointment)
        {
            try
            {
                // Create in-app notification
                var notification = new NotificationDto
                {
                    UserId = appointment.ClientId,
                    Title = "Appointment Updated",
                    Message = $"Your appointment with {appointment.DoctorName} has been updated. New date: {appointment.AppointmentDate} at {appointment.AppointmentTime}.",
                    Type = NotificationType.AppointmentModified,
                    AppointmentId = appointment.Id
                };

                await CreateNotificationAsync(notification);

                // Send email notification
                var emailSent = await _emailService.SendAppointmentModifiedEmailAsync(appointment);
                notification.EmailSent = emailSent;

                _logger.LogInformation("Appointment modified notifications sent for appointment {AppointmentId}", appointment.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send appointment modified notifications for appointment {AppointmentId}", appointment.Id);
                return false;
            }
        }

        public async Task<bool> SendAppointmentReminderNotificationAsync(AppointmentResponseDto appointment)
        {
            try
            {
                // Create in-app notification
                var notification = new NotificationDto
                {
                    UserId = appointment.ClientId,
                    Title = "Appointment Reminder",
                    Message = $"Reminder: You have an appointment with {appointment.DoctorName} in 1 hour at {appointment.AppointmentTime}.",
                    Type = NotificationType.AppointmentReminder,
                    AppointmentId = appointment.Id
                };

                await CreateNotificationAsync(notification);

                // Send email notification
                var emailSent = await _emailService.SendAppointmentReminderEmailAsync(appointment);
                notification.EmailSent = emailSent;

                _logger.LogInformation("Appointment reminder notifications sent for appointment {AppointmentId}", appointment.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send appointment reminder notifications for appointment {AppointmentId}", appointment.Id);
                return false;
            }
        }

        public async Task<bool> SendAppointmentCancelledNotificationAsync(AppointmentResponseDto appointment)
        {
            try
            {
                // Create in-app notification
                var notification = new NotificationDto
                {
                    UserId = appointment.ClientId,
                    Title = "Appointment Cancelled",
                    Message = $"Your appointment with {appointment.DoctorName} on {appointment.AppointmentDate} at {appointment.AppointmentTime} has been cancelled.",
                    Type = NotificationType.AppointmentCancelled,
                    AppointmentId = appointment.Id
                };

                await CreateNotificationAsync(notification);

                // Send email notification
                var emailSent = await _emailService.SendAppointmentCancelledEmailAsync(appointment);
                notification.EmailSent = emailSent;

                _logger.LogInformation("Appointment cancelled notifications sent for appointment {AppointmentId}", appointment.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send appointment cancelled notifications for appointment {AppointmentId}", appointment.Id);
                return false;
            }
        }
    }
}