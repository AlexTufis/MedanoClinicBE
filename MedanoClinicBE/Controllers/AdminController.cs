using MedanoClinicBE.DTOs;
using MedanoClinicBE.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedanoClinicBE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet("dashboard")]
        public async Task<ActionResult<AdminDashboardDto>> GetDashboardStatistics()
        {
            try
            {
                var statistics = await _adminService.GetDashboardStatisticsAsync();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpGet("users")]
        public async Task<ActionResult<List<UserDto>>> GetAllUsers()
        {
            try
            {
                var users = await _adminService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpGet("getAppointments")]
        public async Task<ActionResult<List<AppointmentResponseDto>>> GetAppointments()
        {
            try
            {
                var appointments = await _adminService.GetAllAppointmentsAsync();
                return Ok(appointments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // Appointment Hours Management Endpoints
        [HttpGet("appointment-hours")]
        public async Task<ActionResult<List<DoctorAppointmentHoursDto>>> GetAllDoctorsAppointmentHours()
        {
            try
            {
                var doctorsHours = await _adminService.GetAllDoctorsAppointmentHoursAsync();
                return Ok(doctorsHours);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpPost("appointment-hours")]
        public async Task<ActionResult<AppointmentHourDto>> CreateAppointmentHour(CreateAppointmentHourDto dto)
        {
            try
            {
                var appointmentHour = await _adminService.CreateAppointmentHourAsync(dto);
                return Ok(appointmentHour);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpPut("appointment-hours/{id}")]
        public async Task<ActionResult<AppointmentHourDto>> UpdateAppointmentHour(string id, UpdateAppointmentHourDto dto)
        {
            try
            {
                var appointmentHour = await _adminService.UpdateAppointmentHourAsync(id, dto);
                return Ok(appointmentHour);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpDelete("appointment-hours/{id}")]
        public async Task<ActionResult> DeleteAppointmentHour(string id)
        {
            try
            {
                var success = await _adminService.DeleteAppointmentHourAsync(id);
                if (!success)
                {
                    return NotFound(new { message = "Appointment hour not found" });
                }
                return Ok(new { message = "Appointment hour deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }
}