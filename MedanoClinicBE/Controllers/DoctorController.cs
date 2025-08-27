using MedanoClinicBE.DTOs;
using MedanoClinicBE.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MedanoClinicBE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Doctor,Admin")]
    public class DoctorController : ControllerBase
    {
        private readonly IDoctorService _doctorService;
        private readonly ILogger<DoctorController> _logger;

        public DoctorController(IDoctorService doctorService, ILogger<DoctorController> logger)
        {
            _doctorService = doctorService;
            _logger = logger;
        }

        [HttpPut("update-user-role")]
        [Authorize(Roles = "Admin")] // Only admins can update roles
        public async Task<IActionResult> UpdateUserRole(UpdateUserRoleDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Additional validation for Doctor role
                if (dto.RoleName.Equals("Doctor", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrWhiteSpace(dto.Specialization))
                    {
                        return BadRequest(new { message = "Specialization is required when assigning Doctor role." });
                    }
                }

                var result = await _doctorService.UpdateUserRoleAsync(dto);
                
                if (result)
                {
                    _logger.LogInformation("User role updated successfully for user {UserId} to role {RoleName}", 
                        dto.UserId, dto.RoleName);
                    
                    var message = dto.RoleName.Equals("Doctor", StringComparison.OrdinalIgnoreCase) 
                        ? "User role updated successfully, doctor record created, and default appointment hours set." 
                        : "User role updated successfully";
                        
                    return Ok(new { message = message });
                }
                else
                {
                    _logger.LogWarning("Failed to update user role for user {UserId} to role {RoleName}", 
                        dto.UserId, dto.RoleName);
                    return BadRequest(new { message = "Failed to update user role. User or role may not exist, or specialization may be missing for Doctor role." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating user role for user {UserId}", dto.UserId);
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpGet("doctors")]
        public async Task<ActionResult<List<DoctorDto>>> GetDoctors()
        {
            try
            {
                var doctors = await _doctorService.GetAllDoctorsAsync();
                return Ok(doctors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving doctors");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpGet("{doctorId}/reviews")]
        public async Task<ActionResult<List<ReviewDto>>> GetDoctorReviews(int doctorId)
        {
            try
            {
                var reviews = await _doctorService.GetDoctorReviewsAsync(doctorId);
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving reviews for doctor {DoctorId}", doctorId);
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpGet("{doctorId}/appointments")]
        public async Task<ActionResult<List<AppointmentResponseDto>>> GetDoctorAppointments(int doctorId)
        {
            try
            {
                var appointments = await _doctorService.GetDoctorAppointmentsAsync(doctorId);
                return Ok(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving appointments for doctor {DoctorId}", doctorId);
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // NEW: Endpoints that work with JWT token (for current doctor) or payload (for admin queries)
        [HttpGet("my-reviews")]
        [Authorize(Roles = "Doctor")] // Only doctors can get their own reviews
        public async Task<ActionResult<List<ReviewDto>>> GetMyReviews()
        {
            try
            {
                // Get the current user's ID from the JWT token
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Unable to identify user");
                }

                var reviews = await _doctorService.GetDoctorReviewsByUserIdAsync(userId);
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving reviews for current doctor");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpGet("my-appointments")]
        [Authorize(Roles = "Doctor")] // Only doctors can get their own appointments
        public async Task<ActionResult<List<AppointmentResponseDto>>> GetMyAppointments()
        {
            try
            {
                // Get the current user's ID from the JWT token
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Unable to identify user");
                }

                var appointments = await _doctorService.GetDoctorAppointmentsByUserIdAsync(userId);
                return Ok(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving appointments for current doctor");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpPost("reviews-by-userid")]
        [Authorize(Roles = "Admin")] // Only admins can query specific doctors by userId
        public async Task<ActionResult<List<ReviewDto>>> GetDoctorReviewsByUserId(DoctorIdRequestDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var reviews = await _doctorService.GetDoctorReviewsByUserIdAsync(dto.DoctorUserId);
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving reviews for doctor with UserId {UserId}", dto.DoctorUserId);
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpPost("appointments-by-userid")]
        [Authorize(Roles = "Admin")] // Only admins can query specific doctors by userId
        public async Task<ActionResult<List<AppointmentResponseDto>>> GetDoctorAppointmentsByUserId(DoctorIdRequestDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var appointments = await _doctorService.GetDoctorAppointmentsByUserIdAsync(dto.DoctorUserId);
                return Ok(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving appointments for doctor with UserId {UserId}", dto.DoctorUserId);
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }
}