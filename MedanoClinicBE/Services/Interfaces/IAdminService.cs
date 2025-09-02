using MedanoClinicBE.DTOs;

namespace MedanoClinicBE.Services.Interfaces
{
    public interface IAdminService
    {
        Task<AdminDashboardDto> GetDashboardStatisticsAsync();
        Task<List<UserDto>> GetAllUsersAsync();
        Task<List<AppointmentResponseDto>> GetAllAppointmentsAsync();
        
        // Appointment Management
        Task<AppointmentResponseDto?> UpdateAppointmentStatusAsync(string appointmentId, UpdateAppointmentStatusDto dto);
        
        // Appointment Hours Management
        Task<AppointmentHourDto> CreateAppointmentHourAsync(CreateAppointmentHourDto dto);
        Task<AppointmentHourDto> UpdateAppointmentHourAsync(string id, UpdateAppointmentHourDto dto);
        Task<bool> DeleteAppointmentHourAsync(string id);
        Task<List<DoctorAppointmentHoursDto>> GetAllDoctorsAppointmentHoursAsync();
    }
}