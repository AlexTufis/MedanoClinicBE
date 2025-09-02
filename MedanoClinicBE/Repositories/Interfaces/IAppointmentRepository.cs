using MedanoClinicBE.DTOs;
using MedanoClinicBE.Models;

namespace MedanoClinicBE.Repositories.Interfaces
{
    public interface IAppointmentRepository
    {
        Task<int> GetTotalAppointmentsCountAsync();
        Task<int> GetTodayAppointmentsCountAsync();
        Task<int> GetWeeklyAppointmentsCountAsync();
        Task<int> GetCompletedAppointmentsCountAsync();
        Task<int> GetAppointmentCountByStatusAsync(AppointmentStatus status);
        Task<AppointmentResponseDto> CreateAppointmentAsync(CreateAppointmentDto dto, string clientId);
        Task<List<AppointmentResponseDto>> GetClientAppointmentsAsync(string clientId);
        Task<List<AppointmentResponseDto>> GetAllAppointmentsAsync();
        Task<List<AppointmentResponseDto>> GetDoctorAppointmentsAsync(int doctorId);
        Task<int> UpdatePastAppointmentsStatusAsync();
        Task<AppointmentResponseDto?> UpdateAppointmentStatusAsync(string appointmentId, UpdateAppointmentStatusDto dto);
    }
}