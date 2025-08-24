using MedanoClinicBE.DTOs;

namespace MedanoClinicBE.Services.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(EmailNotificationDto emailDto);
        Task<bool> SendAppointmentCreatedEmailAsync(AppointmentResponseDto appointment);
        Task<bool> SendAppointmentModifiedEmailAsync(AppointmentResponseDto appointment);
        Task<bool> SendAppointmentReminderEmailAsync(AppointmentResponseDto appointment);
        Task<bool> SendAppointmentCancelledEmailAsync(AppointmentResponseDto appointment);
    }
}