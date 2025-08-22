using MedanoClinicBE.DTOs;

namespace MedanoClinicBE.Services.Interfaces
{
    public interface IAdminService
    {
        Task<AdminDashboardDto> GetDashboardStatisticsAsync();
        Task<List<UserDto>> GetAllUsersAsync();
    }
}