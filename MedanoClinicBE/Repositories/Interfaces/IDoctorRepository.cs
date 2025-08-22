using MedanoClinicBE.DTOs;

namespace MedanoClinicBE.Repositories.Interfaces
{
    public interface IDoctorRepository
    {
        Task<List<DoctorDto>> GetAllActiveDoctorsAsync();
    }
}