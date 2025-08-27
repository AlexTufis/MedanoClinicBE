using MedanoClinicBE.DTOs;
using MedanoClinicBE.Models;

namespace MedanoClinicBE.Repositories.Interfaces
{
    public interface IDoctorRepository
    {
        Task<List<DoctorDto>> GetAllActiveDoctorsAsync();
        Task<Doctor?> GetDoctorByUserIdAsync(string userId);
        Task<Doctor> CreateDoctorAsync(string userId, string specialization, string? phone = null);
    }
}