using MedanoClinicBE.Data;
using MedanoClinicBE.DTOs;
using MedanoClinicBE.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MedanoClinicBE.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Models.ApplicationUser> _userManager;

        public UserRepository(ApplicationDbContext context, UserManager<Models.ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<int> GetTotalUsersCountAsync()
        {
            return await _context.Users.CountAsync();
        }

        public async Task<int> GetUserCountByRoleAsync(string role)
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(role);
            return usersInRole.Count;
        }

        public async Task<int> GetNewUsersThisMonthAsync()
        {
            var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1);
            
            return await _context.Users
                .Where(u => u.CreatedAt >= startOfMonth && u.CreatedAt < endOfMonth)
                .CountAsync();
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            var users = await _context.Users.ToListAsync();
            var userDtos = new List<UserDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var userRole = roles.FirstOrDefault() ?? "Client";

                // Only include Admin and Client users as per frontend interface
                if (userRole == "Admin" || userRole == "Client")
                {
                    userDtos.Add(new UserDto
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        DisplayName = user.DisplayName,
                        DateOfBirth = user.DateOfBirth?.ToString("yyyy-MM-dd"),
                        Gender = (int?)user.Gender,
                        Role = userRole,
                        CreatedAt = user.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                        IsActive = !user.LockoutEnabled || user.LockoutEnd == null || user.LockoutEnd <= DateTime.UtcNow
                    });
                }
            }

            return userDtos;
        }
    }
}