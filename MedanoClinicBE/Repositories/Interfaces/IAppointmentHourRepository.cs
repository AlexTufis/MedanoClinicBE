using MedanoClinicBE.DTOs;

namespace MedanoClinicBE.Repositories.Interfaces
{
    public interface IAppointmentHourRepository
    {
        Task<List<AppointmentHourDto>> GetDoctorAppointmentHoursAsync(int doctorId);
        Task<List<AppointmentHourDto>> GetDoctorAppointmentHoursByDayAsync(int doctorId, DayOfWeek dayOfWeek);
        Task<List<DoctorAppointmentHoursDto>> GetAllDoctorsAppointmentHoursAsync();
        Task<AppointmentHourDto> CreateAppointmentHourAsync(CreateAppointmentHourDto dto);
        Task<AppointmentHourDto> UpdateAppointmentHourAsync(int id, UpdateAppointmentHourDto dto);
        Task<bool> DeleteAppointmentHourAsync(int id);
        Task<AppointmentHourDto?> GetAppointmentHourByIdAsync(int id);
        Task<bool> AppointmentHourExistsAsync(int doctorId, TimeSpan hour, DayOfWeek dayOfWeek);
        Task CreateDefaultAppointmentHoursForDoctorAsync(int doctorId);
    }
}