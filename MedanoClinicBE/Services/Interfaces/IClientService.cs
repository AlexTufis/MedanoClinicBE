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
    }
}