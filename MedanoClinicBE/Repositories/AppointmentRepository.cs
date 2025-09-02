using MedanoClinicBE.Data;
using MedanoClinicBE.DTOs;
using MedanoClinicBE.Models;
using MedanoClinicBE.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MedanoClinicBE.Repositories
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly ApplicationDbContext _context;

        public AppointmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetTotalAppointmentsCountAsync()
        {
            return await _context.Appointments.CountAsync();
        }

        public async Task<int> GetTodayAppointmentsCountAsync()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            
            return await _context.Appointments
                .Where(a => a.AppointmentDate >= today && a.AppointmentDate < tomorrow)
                .CountAsync();
        }

        public async Task<int> GetWeeklyAppointmentsCountAsync()
        {
            var startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(7);
            
            return await _context.Appointments
                .Where(a => a.CreatedAt >= startOfWeek && a.CreatedAt < endOfWeek)
                .CountAsync();
        }

        public async Task<int> GetCompletedAppointmentsCountAsync()
        {
            return await _context.Appointments
                .Where(a => a.Status == AppointmentStatus.Completed)
                .CountAsync();
        }

        public async Task<int> GetAppointmentCountByStatusAsync(AppointmentStatus status)
        {
            return await _context.Appointments
                .Where(a => a.Status == status)
                .CountAsync();
        }

        public async Task<AppointmentResponseDto> CreateAppointmentAsync(CreateAppointmentDto dto, string clientId)
        {
            // Parse the string inputs to proper types
            var appointmentDate = DateTime.ParseExact(dto.AppointmentDate, "yyyy-MM-dd", null);
            var appointmentTime = TimeSpan.ParseExact(dto.AppointmentTime, @"hh\:mm", null);
            var doctorId = int.Parse(dto.DoctorId);

            var appointment = new Appointment
            {
                PatientId = clientId,
                DoctorId = doctorId,
                AppointmentDate = appointmentDate,
                AppointmentTime = appointmentTime,
                Reason = dto.Reason,
                Notes = dto.Notes,
                Status = AppointmentStatus.Scheduled,
                CreatedAt = DateTime.UtcNow
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            // Fetch the appointment with related data
            var createdAppointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
                .FirstOrDefaultAsync(a => a.Id == appointment.Id);

            // Convert status enum to lowercase string to match frontend
            var statusString = createdAppointment.Status switch
            {
                AppointmentStatus.Scheduled => "scheduled",
                AppointmentStatus.Completed => "completed",
                AppointmentStatus.Cancelled => "cancelled",
                AppointmentStatus.NoShow => "no-show",
                _ => "scheduled"
            };

            var appointmentDto = new AppointmentResponseDto
            {
                Id = createdAppointment.Id.ToString(),
                ClientId = createdAppointment.PatientId,
                ClientName = $"{createdAppointment.Patient.FirstName} {createdAppointment.Patient.LastName}",
                DoctorId = createdAppointment.DoctorId.ToString(),
                DoctorName = $"{createdAppointment.Doctor.User.FirstName} {createdAppointment.Doctor.User.LastName}",
                DoctorSpecialization = createdAppointment.Doctor.Specialization,
                AppointmentDate = createdAppointment.AppointmentDate.ToString("yyyy-MM-dd"),
                AppointmentTime = createdAppointment.AppointmentTime.ToString(@"hh\:mm"),
                Status = statusString,
                Reason = createdAppointment.Reason,
                Notes = createdAppointment.Notes,
                CreatedAt = createdAppointment.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
            };

            return appointmentDto;
        }

        public async Task<List<AppointmentResponseDto>> GetClientAppointmentsAsync(string clientId)
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
                .Where(a => a.PatientId == clientId)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            var appointmentDtos = new List<AppointmentResponseDto>();

            foreach (var appointment in appointments)
            {
                // Convert status enum to lowercase string to match frontend
                var statusString = appointment.Status switch
                {
                    AppointmentStatus.Scheduled => "scheduled",
                    AppointmentStatus.Completed => "completed",
                    AppointmentStatus.Cancelled => "cancelled",
                    AppointmentStatus.NoShow => "no-show",
                    AppointmentStatus.InProgress => "scheduled", // Map InProgress to scheduled for frontend
                    _ => "scheduled"
                };

                appointmentDtos.Add(new AppointmentResponseDto
                {
                    Id = appointment.Id.ToString(),
                    ClientId = appointment.PatientId,
                    ClientName = $"{appointment.Patient.FirstName} {appointment.Patient.LastName}",
                    DoctorId = appointment.DoctorId.ToString(),
                    DoctorName = $"{appointment.Doctor.User.FirstName} {appointment.Doctor.User.LastName}",
                    DoctorSpecialization = appointment.Doctor.Specialization,
                    AppointmentDate = appointment.AppointmentDate.ToString("yyyy-MM-dd"),
                    AppointmentTime = appointment.AppointmentTime.ToString(@"hh\:mm"),
                    Status = statusString,
                    Reason = appointment.Reason,
                    Notes = appointment.Notes,
                    CreatedAt = appointment.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                });
            }

            return appointmentDtos;
        }

        public async Task<List<AppointmentResponseDto>> GetAllAppointmentsAsync()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            var appointmentDtos = new List<AppointmentResponseDto>();

            foreach (var appointment in appointments)
            {
                // Convert status enum to lowercase string to match frontend
                var statusString = appointment.Status switch
                {
                    AppointmentStatus.Scheduled => "scheduled",
                    AppointmentStatus.Completed => "completed",
                    AppointmentStatus.Cancelled => "cancelled",
                    AppointmentStatus.NoShow => "no-show",
                    AppointmentStatus.InProgress => "scheduled", // Map InProgress to scheduled for frontend
                    _ => "scheduled"
                };

                appointmentDtos.Add(new AppointmentResponseDto
                {
                    Id = appointment.Id.ToString(),
                    ClientId = appointment.PatientId,
                    ClientName = $"{appointment.Patient.FirstName} {appointment.Patient.LastName}",
                    DoctorId = appointment.DoctorId.ToString(),
                    DoctorName = $"{appointment.Doctor.User.FirstName} {appointment.Doctor.User.LastName}",
                    DoctorSpecialization = appointment.Doctor.Specialization,
                    AppointmentDate = appointment.AppointmentDate.ToString("yyyy-MM-dd"),
                    AppointmentTime = appointment.AppointmentTime.ToString(@"hh\:mm"),
                    Status = statusString,
                    Reason = appointment.Reason,
                    Notes = appointment.Notes,
                    CreatedAt = appointment.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                });
            }

            return appointmentDtos;
        }

        public async Task<List<AppointmentResponseDto>> GetDoctorAppointmentsAsync(int doctorId)
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
                .Where(a => a.DoctorId == doctorId)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            var appointmentDtos = new List<AppointmentResponseDto>();

            foreach (var appointment in appointments)
            {
                // Convert status enum to lowercase string to match frontend
                var statusString = appointment.Status switch
                {
                    AppointmentStatus.Scheduled => "scheduled",
                    AppointmentStatus.Completed => "completed",
                    AppointmentStatus.Cancelled => "cancelled",
                    AppointmentStatus.NoShow => "no-show",
                    AppointmentStatus.InProgress => "scheduled", // Map InProgress to scheduled for frontend
                    _ => "scheduled"
                };

                appointmentDtos.Add(new AppointmentResponseDto
                {
                    Id = appointment.Id.ToString(),
                    ClientId = appointment.PatientId,
                    ClientName = $"{appointment.Patient.FirstName} {appointment.Patient.LastName}",
                    DoctorId = appointment.DoctorId.ToString(),
                    DoctorName = $"{appointment.Doctor.User.FirstName} {appointment.Doctor.User.LastName}",
                    DoctorSpecialization = appointment.Doctor.Specialization,
                    AppointmentDate = appointment.AppointmentDate.ToString("yyyy-MM-dd"),
                    AppointmentTime = appointment.AppointmentTime.ToString(@"hh\:mm"),
                    Status = statusString,
                    Reason = appointment.Reason,
                    Notes = appointment.Notes,
                    CreatedAt = appointment.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                });
            }

            return appointmentDtos;
        }

        public async Task<int> UpdatePastAppointmentsStatusAsync()
        {
            // Get current date and time for comparison
            var currentDate = DateTime.Today;
            var currentTime = DateTime.Now.TimeOfDay;

            // Find appointments that are past their scheduled time and still scheduled
            var pastAppointments = await _context.Appointments
                .Where(a => a.Status == AppointmentStatus.Scheduled && 
                           (a.AppointmentDate < currentDate || 
                            (a.AppointmentDate == currentDate && a.AppointmentTime < currentTime)))
                .ToListAsync();

            if (pastAppointments.Any())
            {
                // Update all past appointments to Completed status
                foreach (var appointment in pastAppointments)
                {
                    appointment.Status = AppointmentStatus.Completed;
                    appointment.CompletedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
            }

            return pastAppointments.Count;
        }

        public async Task<AppointmentResponseDto?> UpdateAppointmentStatusAsync(string appointmentId, UpdateAppointmentStatusDto dto)
        {
            var id = int.Parse(appointmentId);
            
            // Find the appointment with related data
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
            {
                return null;
            }

            // Parse the status string to enum
            var newStatus = dto.Status.ToLower() switch
            {
                "scheduled" => AppointmentStatus.Scheduled,
                "completed" => AppointmentStatus.Completed,
                "cancelled" => AppointmentStatus.Cancelled,
                "no-show" => AppointmentStatus.NoShow,
                "in-progress" => AppointmentStatus.InProgress,
                _ => throw new ArgumentException($"Invalid status: {dto.Status}")
            };

            // Update the appointment
            appointment.Status = newStatus;
            
            // Set CompletedAt timestamp if marking as completed
            if (newStatus == AppointmentStatus.Completed && appointment.CompletedAt == null)
            {
                appointment.CompletedAt = DateTime.UtcNow;
            }
            
            // Add admin notes to the existing notes if provided
            if (!string.IsNullOrEmpty(dto.AdminNotes))
            {
                appointment.Notes = string.IsNullOrEmpty(appointment.Notes) 
                    ? $"Admin: {dto.AdminNotes}" 
                    : $"{appointment.Notes}\nAdmin: {dto.AdminNotes}";
            }

            await _context.SaveChangesAsync();

            // Convert status enum to lowercase string for response
            var statusString = appointment.Status switch
            {
                AppointmentStatus.Scheduled => "scheduled",
                AppointmentStatus.Completed => "completed",
                AppointmentStatus.Cancelled => "cancelled",
                AppointmentStatus.NoShow => "no-show",
                AppointmentStatus.InProgress => "in-progress",
                _ => "scheduled"
            };

            return new AppointmentResponseDto
            {
                Id = appointment.Id.ToString(),
                ClientId = appointment.PatientId,
                ClientName = $"{appointment.Patient.FirstName} {appointment.Patient.LastName}",
                DoctorId = appointment.DoctorId.ToString(),
                DoctorName = $"{appointment.Doctor.User.FirstName} {appointment.Doctor.User.LastName}",
                DoctorSpecialization = appointment.Doctor.Specialization,
                AppointmentDate = appointment.AppointmentDate.ToString("yyyy-MM-dd"),
                AppointmentTime = appointment.AppointmentTime.ToString(@"hh\:mm"),
                Status = statusString,
                Reason = appointment.Reason,
                Notes = appointment.Notes,
                CreatedAt = appointment.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
            };
        }
    }
}