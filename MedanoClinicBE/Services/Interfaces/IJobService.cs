using MedanoClinicBE.DTOs;
using MedanoClinicBE.Services.Interfaces;

namespace MedanoClinicBE.Services.Interfaces
{
    public interface IJobService
    {
        void ScheduleAppointmentReminder(AppointmentResponseDto appointment);
        void CancelAppointmentReminder(string appointmentId);
        Task ProcessAppointmentReminderAsync(string appointmentId);
        
        // New method for sending appointment creation emails
        void SendAppointmentCreatedEmailJob(AppointmentResponseDto appointment);
        Task ProcessAppointmentCreatedEmailAsync(string appointmentId);
    }
}