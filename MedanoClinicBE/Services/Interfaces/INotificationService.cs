using MedanoClinicBE.DTOs;

namespace MedanoClinicBE.Services.Interfaces
{
    public interface INotificationService
    {
        Task<NotificationDto> CreateNotificationAsync(NotificationDto notification);
        Task<List<NotificationDto>> GetUserNotificationsAsync(string userId);
        Task<bool> MarkNotificationAsReadAsync(string notificationId);
        Task<bool> SendAppointmentCreatedNotificationAsync(AppointmentResponseDto appointment);
        Task<bool> SendAppointmentModifiedNotificationAsync(AppointmentResponseDto appointment);
        Task<bool> SendAppointmentReminderNotificationAsync(AppointmentResponseDto appointment);
        Task<bool> SendAppointmentCancelledNotificationAsync(AppointmentResponseDto appointment);
    }
}