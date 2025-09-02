using MedanoClinicBE.DTOs;
using MedanoClinicBE.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MedanoClinicBE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Client,Admin")]
    public class ClientController : ControllerBase
    {
        private readonly IClientService _clientService;

        public ClientController(IClientService clientService)
        {
            _clientService = clientService;
        }

        [HttpGet("doctors")]
        public async Task<ActionResult<List<DoctorDto>>> GetDoctors()
        {
            try
            {
                var doctors = await _clientService.GetDoctorsAsync();
                return Ok(doctors);
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
                // Get the current user's ID from the JWT token
                var clientId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(clientId))
                {
                    return Unauthorized("Unable to identify user");
                }

                var appointments = await _clientService.GetClientAppointmentsAsync(clientId);
                return Ok(appointments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpGet("getReviews")]
        public async Task<ActionResult<List<ReviewDto>>> GetReviews()
        {
            try
            {
                var reviews = await _clientService.GetAllReviewsAsync();
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpPost("appointments")]
        public async Task<ActionResult<AppointmentResponseDto>> CreateAppointment(CreateAppointmentDto dto)
        {
            try
            {
                // Get the current user's ID from the JWT token
                var clientId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(clientId))
                {
                    return Unauthorized("Unable to identify user");
                }

                var appointment = await _clientService.CreateAppointmentAsync(dto, clientId);
                return Ok(appointment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpPost("reviews")]
        public async Task<ActionResult<ReviewDto>> AddReview(CreateReviewDto dto)
        {
            try
            {
                // Get the current user's ID from the JWT token
                var clientId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(clientId))
                {
                    return Unauthorized("Unable to identify user");
                }

                var review = await _clientService.CreateReviewAsync(dto, clientId);
                return Ok(review);
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

        // Appointment Hours Management Endpoints
        [HttpGet("appointment-hours")]
        public async Task<ActionResult<List<DoctorAppointmentHoursDto>>> GetAllDoctorsAppointmentHours()
        {
            try
            {
                var doctorsHours = await _clientService.GetAllDoctorsAppointmentHoursAsync();
                return Ok(doctorsHours);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpGet("appointment-hours/doctor/{doctorId}")]
        public async Task<ActionResult<List<AppointmentHourDto>>> GetDoctorAppointmentHours(string doctorId)
        {
            try
            {
                var appointmentHours = await _clientService.GetDoctorAppointmentHoursAsync(doctorId);
                return Ok(appointmentHours);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpGet("appointment-hours/doctor/{doctorId}/day/{dayOfWeek}")]
        public async Task<ActionResult<List<AppointmentHourDto>>> GetDoctorAppointmentHoursByDay(string doctorId, string dayOfWeek)
        {
            try
            {
                var appointmentHours = await _clientService.GetDoctorAppointmentHoursByDayAsync(doctorId, dayOfWeek);
                return Ok(appointmentHours);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }
}