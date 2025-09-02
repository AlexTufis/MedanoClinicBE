using MedanoClinicBE.DTOs;
using MedanoClinicBE.Models;
using MedanoClinicBE.Repositories.Interfaces;
using MedanoClinicBE.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace MedanoClinicBE.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IReviewRepository _reviewRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IAppointmentHourRepository _appointmentHourRepository;
        private readonly IMedicalReportRepository _medicalReportRepository;
        private readonly ILogger<DoctorService> _logger;

        public DoctorService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IDoctorRepository doctorRepository,
            IReviewRepository reviewRepository,
            IAppointmentRepository appointmentRepository,
            IAppointmentHourRepository appointmentHourRepository,
            IMedicalReportRepository medicalReportRepository,
            ILogger<DoctorService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _doctorRepository = doctorRepository;
            _reviewRepository = reviewRepository;
            _appointmentRepository = appointmentRepository;
            _appointmentHourRepository = appointmentHourRepository;
            _medicalReportRepository = medicalReportRepository;
            _logger = logger;
        }

        public async Task<bool> UpdateUserRoleAsync(UpdateUserRoleDto dto)
        {
            try
            {
                // Find the user
                var user = await _userManager.FindByIdAsync(dto.UserId);
                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found", dto.UserId);
                    return false;
                }

                // Check if the role exists
                var roleExists = await _roleManager.RoleExistsAsync(dto.RoleName);
                if (!roleExists)
                {
                    _logger.LogWarning("Role {RoleName} does not exist", dto.RoleName);
                    return false;
                }

                // Validate Doctor role requirements
                if (dto.RoleName.Equals("Doctor", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrWhiteSpace(dto.Specialization))
                    {
                        _logger.LogWarning("Specialization is required when assigning Doctor role to user {UserId}", dto.UserId);
                        return false;
                    }
                }

                // Get current user roles
                var currentRoles = await _userManager.GetRolesAsync(user);

                // Remove user from all current roles
                if (currentRoles.Any())
                {
                    var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    if (!removeResult.Succeeded)
                    {
                        _logger.LogError("Failed to remove user {UserId} from current roles: {Errors}", 
                            dto.UserId, string.Join(", ", removeResult.Errors.Select(e => e.Description)));
                        return false;
                    }
                }

                // Add user to new role
                var addResult = await _userManager.AddToRoleAsync(user, dto.RoleName);
                if (!addResult.Succeeded)
                {
                    _logger.LogError("Failed to add user {UserId} to role {RoleName}: {Errors}", 
                        dto.UserId, dto.RoleName, string.Join(", ", addResult.Errors.Select(e => e.Description)));
                    return false;
                }

                // Create doctor record if role is Doctor
                if (dto.RoleName.Equals("Doctor", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        // Check if doctor record already exists
                        var existingDoctor = await _doctorRepository.GetDoctorByUserIdAsync(dto.UserId);
                        if (existingDoctor == null)
                        {
                            // Create new doctor record
                            var newDoctor = await _doctorRepository.CreateDoctorAsync(dto.UserId, dto.Specialization!, dto.Phone);
                            _logger.LogInformation("Doctor record created for user {UserId} with specialization {Specialization}", 
                                dto.UserId, dto.Specialization);

                            // Create default appointment hours for the new doctor
                            try
                            {
                                await _appointmentHourRepository.CreateDefaultAppointmentHoursForDoctorAsync(newDoctor.Id);
                                _logger.LogInformation("Default appointment hours created for doctor {DoctorId}", newDoctor.Id);
                            }
                            catch (Exception ahEx)
                            {
                                _logger.LogError(ahEx, "Failed to create default appointment hours for doctor {DoctorId}", newDoctor.Id);
                                // Continue - doctor creation succeeded, appointment hours can be created later
                            }
                        }
                        else
                        {
                            _logger.LogInformation("Doctor record already exists for user {UserId}", dto.UserId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to create doctor record for user {UserId}", dto.UserId);
                        // Consider if you want to rollback the role assignment here
                        // For now, we'll continue as the role assignment succeeded
                    }
                }

                _logger.LogInformation("Successfully updated user {UserId} to role {RoleName}", dto.UserId, dto.RoleName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user role for user {UserId} to role {RoleName}", dto.UserId, dto.RoleName);
                return false;
            }
        }

        public async Task<List<DoctorDto>> GetAllDoctorsAsync()
        {
            try
            {
                return await _doctorRepository.GetAllActiveDoctorsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all doctors");
                throw;
            }
        }

        public async Task<List<ReviewDto>> GetDoctorReviewsAsync(int doctorId)
        {
            try
            {
                return await _reviewRepository.GetDoctorReviewsAsync(doctorId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reviews for doctor {DoctorId}", doctorId);
                throw;
            }
        }

        public async Task<List<AppointmentResponseDto>> GetDoctorAppointmentsAsync(int doctorId)
        {
            try
            {
                return await _appointmentRepository.GetDoctorAppointmentsAsync(doctorId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving appointments for doctor {DoctorId}", doctorId);
                throw;
            }
        }

        public async Task<List<ReviewDto>> GetDoctorReviewsByUserIdAsync(string userId)
        {
            try
            {
                // Find the doctor by UserId
                var doctor = await _doctorRepository.GetDoctorByUserIdAsync(userId);
                if (doctor == null)
                {
                    _logger.LogWarning("Doctor not found for UserId {UserId}", userId);
                    return new List<ReviewDto>();
                }

                return await _reviewRepository.GetDoctorReviewsAsync(doctor.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reviews for doctor with UserId {UserId}", userId);
                throw;
            }
        }

        public async Task<List<AppointmentResponseDto>> GetDoctorAppointmentsByUserIdAsync(string userId)
        {
            try
            {
                // Find the doctor by UserId
                var doctor = await _doctorRepository.GetDoctorByUserIdAsync(userId);
                if (doctor == null)
                {
                    _logger.LogWarning("Doctor not found for UserId {UserId}", userId);
                    return new List<AppointmentResponseDto>();
                }

                return await _appointmentRepository.GetDoctorAppointmentsAsync(doctor.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving appointments for doctor with UserId {UserId}", userId);
                throw;
            }
        }

        // Medical Report methods
        public async Task<MedicalReportDto> CreateMedicalReportAsync(CreateMedicalReportDto dto, string doctorUserId)
        {
            try
            {
                return await _medicalReportRepository.CreateMedicalReportAsync(dto, doctorUserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating medical report for appointment {AppointmentId} by doctor {DoctorUserId}", 
                    dto.AppointmentId, doctorUserId);
                throw;
            }
        }

        public async Task<MedicalReportDto?> UpdateMedicalReportAsync(string reportId, UpdateMedicalReportDto dto, string doctorUserId)
        {
            try
            {
                return await _medicalReportRepository.UpdateMedicalReportAsync(reportId, dto, doctorUserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating medical report {ReportId} by doctor {DoctorUserId}", 
                    reportId, doctorUserId);
                throw;
            }
        }

        public async Task<MedicalReportDto?> GetMedicalReportByIdAsync(string reportId)
        {
            try
            {
                return await _medicalReportRepository.GetMedicalReportByIdAsync(reportId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving medical report {ReportId}", reportId);
                throw;
            }
        }

        public async Task<MedicalReportDto?> GetMedicalReportByAppointmentIdAsync(string appointmentId)
        {
            try
            {
                return await _medicalReportRepository.GetMedicalReportByAppointmentIdAsync(appointmentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving medical report for appointment {AppointmentId}", appointmentId);
                throw;
            }
        }

        public async Task<List<MedicalReportDto>> GetMyMedicalReportsAsync(string doctorUserId)
        {
            try
            {
                return await _medicalReportRepository.GetMedicalReportsByDoctorAsync(doctorUserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving medical reports for doctor {DoctorUserId}", doctorUserId);
                throw;
            }
        }

        public async Task<bool> DeleteMedicalReportAsync(string reportId, string doctorUserId)
        {
            try
            {
                return await _medicalReportRepository.DeleteMedicalReportAsync(reportId, doctorUserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting medical report {ReportId} by doctor {DoctorUserId}", 
                    reportId, doctorUserId);
                throw;
            }
        }
    }
}