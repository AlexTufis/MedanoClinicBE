using MedanoClinicBE.DTOs;

namespace MedanoClinicBE.Services.Interfaces
{
    public interface IDoctorService
    {
        Task<bool> UpdateUserRoleAsync(UpdateUserRoleDto dto);
        Task<List<DoctorDto>> GetAllDoctorsAsync();
        Task<List<ReviewDto>> GetDoctorReviewsAsync(int doctorId);
        Task<List<AppointmentResponseDto>> GetDoctorAppointmentsAsync(int doctorId);
        Task<List<ReviewDto>> GetDoctorReviewsByUserIdAsync(string userId);
        Task<List<AppointmentResponseDto>> GetDoctorAppointmentsByUserIdAsync(string userId);
    }
}