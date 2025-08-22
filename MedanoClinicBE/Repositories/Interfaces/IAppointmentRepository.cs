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
    }
}