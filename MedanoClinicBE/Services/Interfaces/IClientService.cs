using MedanoClinicBE.DTOs;

namespace MedanoClinicBE.Services.Interfaces
{
    public interface IClientService
    {
        Task<List<DoctorDto>> GetDoctorsAsync();
        Task<AppointmentResponseDto> CreateAppointmentAsync(CreateAppointmentDto dto, string clientId);
        Task<List<AppointmentResponseDto>> GetClientAppointmentsAsync(string clientId);
        Task<ReviewDto> CreateReviewAsync(CreateReviewDto dto, string clientId);
        Task<List<ReviewDto>> GetAllReviewsAsync();
        
        // Appointment Hours Management
        Task<List<AppointmentHourDto>> GetDoctorAppointmentHoursAsync(string doctorId);
        Task<List<AppointmentHourDto>> GetDoctorAppointmentHoursByDayAsync(string doctorId, string dayOfWeek);
        Task<List<DoctorAppointmentHoursDto>> GetAllDoctorsAppointmentHoursAsync();
    }
}