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
        private readonly IAppointmentHourRepository _appointmentHourRepository;
        private readonly IMedicalReportRepository _medicalReportRepository;

        public AdminService(
            IUserRepository userRepository, 
            IAppointmentRepository appointmentRepository,
            IAppointmentHourRepository appointmentHourRepository,
            IMedicalReportRepository medicalReportRepository)
        {
            _userRepository = userRepository;
            _appointmentRepository = appointmentRepository;
            _appointmentHourRepository = appointmentHourRepository;
            _medicalReportRepository = medicalReportRepository;
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
            var users = await _userRepository.GetAllUsersAsync();
            
            // For each user, get their medical reports
            foreach (var user in users)
            {
                try
                {
                    user.MedicalReports = await _medicalReportRepository.GetMedicalReportsByPatientAsync(user.Id);
                }
                catch
                {
                    // If there's an error getting medical reports for a user, just leave the list empty
                    user.MedicalReports = new List<MedicalReportDto>();
                }
            }
            
            return users;
        }

        public async Task<List<AppointmentResponseDto>> GetAllAppointmentsAsync()
        {
            return await _appointmentRepository.GetAllAppointmentsAsync();
        }

        // Appointment Management
        public async Task<AppointmentResponseDto?> UpdateAppointmentStatusAsync(string appointmentId, UpdateAppointmentStatusDto dto)
        {
            return await _appointmentRepository.UpdateAppointmentStatusAsync(appointmentId, dto);
        }

        // Appointment Hours Management
        public async Task<AppointmentHourDto> CreateAppointmentHourAsync(CreateAppointmentHourDto dto)
        {
            return await _appointmentHourRepository.CreateAppointmentHourAsync(dto);
        }

        public async Task<AppointmentHourDto> UpdateAppointmentHourAsync(string id, UpdateAppointmentHourDto dto)
        {
            var appointmentHourId = int.Parse(id);
            return await _appointmentHourRepository.UpdateAppointmentHourAsync(appointmentHourId, dto);
        }

        public async Task<bool> DeleteAppointmentHourAsync(string id)
        {
            var appointmentHourId = int.Parse(id);
            return await _appointmentHourRepository.DeleteAppointmentHourAsync(appointmentHourId);
        }

        public async Task<List<DoctorAppointmentHoursDto>> GetAllDoctorsAppointmentHoursAsync()
        {
            return await _appointmentHourRepository.GetAllDoctorsAppointmentHoursAsync();
        }
    }
}