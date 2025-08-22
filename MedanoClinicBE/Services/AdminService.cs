using MedanoClinicBE.DTOs;
using MedanoClinicBE.Models;
using MedanoClinicBE.Repositories.Interfaces;
using MedanoClinicBE.Services.Interfaces;

namespace MedanoClinicBE.Services
{
    public class AdminService : IAdminService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAppointmentRepository _appointmentRepository;

        public AdminService(IUserRepository userRepository, IAppointmentRepository appointmentRepository)
        {
            _userRepository = userRepository;
            _appointmentRepository = appointmentRepository;
        }

        public async Task<AdminDashboardDto> GetDashboardStatisticsAsync()
        {
            var totalUsers = await _userRepository.GetTotalUsersCountAsync();
            var clientUsers = await _userRepository.GetUserCountByRoleAsync("Client");
            var adminUsers = await _userRepository.GetUserCountByRoleAsync("Admin");
            var doctorUsers = await _userRepository.GetUserCountByRoleAsync("Doctor");
            var newUsersThisMonth = await _userRepository.GetNewUsersThisMonthAsync();
            
            var totalAppointments = await _appointmentRepository.GetTotalAppointmentsCountAsync();
            var todayAppointments = await _appointmentRepository.GetTodayAppointmentsCountAsync();
            var weeklyAppointments = await _appointmentRepository.GetWeeklyAppointmentsCountAsync();
            
            // Get appointment counts by status
            var scheduledAppointments = await _appointmentRepository.GetAppointmentCountByStatusAsync(AppointmentStatus.Scheduled);
            var completedAppointments = await _appointmentRepository.GetAppointmentCountByStatusAsync(AppointmentStatus.Completed);
            var cancelledAppointments = await _appointmentRepository.GetAppointmentCountByStatusAsync(AppointmentStatus.Cancelled);
            var noShowAppointments = await _appointmentRepository.GetAppointmentCountByStatusAsync(AppointmentStatus.NoShow);

            return new AdminDashboardDto
            {
                TotalUsers = totalUsers,
                ClientUsers = clientUsers,
                AdminUsers = adminUsers,
                DoctorUsers = doctorUsers,
                NewUsersThisMonth = newUsersThisMonth,
                TotalAppointments = totalAppointments,
                TodayAppointments = todayAppointments,
                WeeklyAppointments = weeklyAppointments,
                AppointmentsByStatus = new AppointmentsByStatus
                {
                    Scheduled = scheduledAppointments,
                    Completed = completedAppointments,
                    Cancelled = cancelledAppointments,
                    NoShow = noShowAppointments
                }
            };
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllUsersAsync();
        }
    }
}