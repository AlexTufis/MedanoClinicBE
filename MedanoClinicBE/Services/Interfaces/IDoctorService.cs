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
        
        // Medical Report methods
        Task<MedicalReportDto> CreateMedicalReportAsync(CreateMedicalReportDto dto, string doctorUserId);
        Task<MedicalReportDto?> UpdateMedicalReportAsync(string reportId, UpdateMedicalReportDto dto, string doctorUserId);
        Task<MedicalReportDto?> GetMedicalReportByIdAsync(string reportId);
        Task<MedicalReportDto?> GetMedicalReportByAppointmentIdAsync(string appointmentId);
        Task<List<MedicalReportDto>> GetMyMedicalReportsAsync(string doctorUserId);
        Task<bool> DeleteMedicalReportAsync(string reportId, string doctorUserId);
    }
}