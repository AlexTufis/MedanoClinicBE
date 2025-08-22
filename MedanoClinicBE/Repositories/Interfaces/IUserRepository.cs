using MedanoClinicBE.DTOs;
using MedanoClinicBE.Models;

namespace MedanoClinicBE.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<int> GetTotalUsersCountAsync();
        Task<int> GetUserCountByRoleAsync(string role);
        Task<int> GetNewUsersThisMonthAsync();
        Task<List<UserDto>> GetAllUsersAsync();
    }
}