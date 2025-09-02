using MedanoClinicBE.DTOs;

namespace MedanoClinicBE.Repositories.Interfaces
{
    public interface IMedicalReportRepository
    {
        Task<MedicalReportDto> CreateMedicalReportAsync(CreateMedicalReportDto dto, string doctorUserId);
        Task<MedicalReportDto?> UpdateMedicalReportAsync(string reportId, UpdateMedicalReportDto dto, string doctorUserId);
        Task<MedicalReportDto?> GetMedicalReportByIdAsync(string reportId);
        Task<MedicalReportDto?> GetMedicalReportByAppointmentIdAsync(string appointmentId);
        Task<List<MedicalReportDto>> GetMedicalReportsByDoctorAsync(string doctorUserId);
        Task<List<MedicalReportDto>> GetMedicalReportsByPatientAsync(string patientUserId);
        Task<bool> DeleteMedicalReportAsync(string reportId, string doctorUserId);
    }
}