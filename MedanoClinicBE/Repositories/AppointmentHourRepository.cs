using MedanoClinicBE.Data;
using MedanoClinicBE.DTOs;
using MedanoClinicBE.Models;
using MedanoClinicBE.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace MedanoClinicBE.Repositories
{
    public class AppointmentHourRepository : IAppointmentHourRepository
    {
        private readonly ApplicationDbContext _context;

        public AppointmentHourRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<AppointmentHourDto>> GetDoctorAppointmentHoursAsync(int doctorId)
        {
            var appointmentHours = await _context.AppointmentHours
                .Where(ah => ah.DoctorId == doctorId)
                .OrderBy(ah => ah.DayOfWeek)
                .ThenBy(ah => ah.Hour)
                .ToListAsync();

            return appointmentHours.Select(ah => new AppointmentHourDto
            {
                Id = ah.Id.ToString(),
                DoctorId = ah.DoctorId.ToString(),
                Hour = ah.Hour.ToString(@"hh\:mm"),
                DayOfWeek = ah.DayOfWeek.ToString(),
                IsActive = ah.IsActive,
                CreatedAt = ah.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                UpdatedAt = ah.UpdatedAt?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
            }).ToList();
        }

        public async Task<List<AppointmentHourDto>> GetDoctorAppointmentHoursByDayAsync(int doctorId, DayOfWeek dayOfWeek)
        {
            var appointmentHours = await _context.AppointmentHours
                .Where(ah => ah.DoctorId == doctorId && ah.DayOfWeek == dayOfWeek && ah.IsActive)
                .OrderBy(ah => ah.Hour)
                .ToListAsync();

            return appointmentHours.Select(ah => new AppointmentHourDto
            {
                Id = ah.Id.ToString(),
                DoctorId = ah.DoctorId.ToString(),
                Hour = ah.Hour.ToString(@"hh\:mm"),
                DayOfWeek = ah.DayOfWeek.ToString(),
                IsActive = ah.IsActive,
                CreatedAt = ah.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                UpdatedAt = ah.UpdatedAt?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
            }).ToList();
        }

        public async Task<List<DoctorAppointmentHoursDto>> GetAllDoctorsAppointmentHoursAsync()
        {
            var doctorsWithHours = await _context.AppointmentHours
                .Include(ah => ah.Doctor)
                .ThenInclude(d => d.User)
                .GroupBy(ah => ah.Doctor)
                .ToListAsync();

            var result = new List<DoctorAppointmentHoursDto>();

            foreach (var group in doctorsWithHours)
            {
                var doctor = group.Key;
                var hours = group.OrderBy(ah => ah.DayOfWeek).ThenBy(ah => ah.Hour).ToList();

                result.Add(new DoctorAppointmentHoursDto
                {
                    DoctorId = doctor.Id.ToString(),
                    DoctorName = $"{doctor.User.FirstName} {doctor.User.LastName}",
                    DoctorSpecialization = doctor.Specialization,
                    AppointmentHours = hours.Select(ah => new AppointmentHourDto
                    {
                        Id = ah.Id.ToString(),
                        DoctorId = ah.DoctorId.ToString(),
                        Hour = ah.Hour.ToString(@"hh\:mm"),
                        DayOfWeek = ah.DayOfWeek.ToString(),
                        IsActive = ah.IsActive,
                        CreatedAt = ah.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                        UpdatedAt = ah.UpdatedAt?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                    }).ToList()
                });
            }

            return result;
        }

        public async Task<AppointmentHourDto> CreateAppointmentHourAsync(CreateAppointmentHourDto dto)
        {
            // Parse the string inputs to proper types
            var hour = TimeSpan.ParseExact(dto.Hour, @"hh\:mm", CultureInfo.InvariantCulture);
            var dayOfWeek = Enum.Parse<DayOfWeek>(dto.DayOfWeek);
            var doctorId = int.Parse(dto.DoctorId);

            var appointmentHour = new AppointmentHour
            {
                DoctorId = doctorId,
                Hour = hour,
                DayOfWeek = dayOfWeek,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.AppointmentHours.Add(appointmentHour);
            await _context.SaveChangesAsync();

            return new AppointmentHourDto
            {
                Id = appointmentHour.Id.ToString(),
                DoctorId = appointmentHour.DoctorId.ToString(),
                Hour = appointmentHour.Hour.ToString(@"hh\:mm"),
                DayOfWeek = appointmentHour.DayOfWeek.ToString(),
                IsActive = appointmentHour.IsActive,
                CreatedAt = appointmentHour.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
            };
        }

        public async Task<AppointmentHourDto> UpdateAppointmentHourAsync(int id, UpdateAppointmentHourDto dto)
        {
            var appointmentHour = await _context.AppointmentHours.FindAsync(id);
            if (appointmentHour == null)
                throw new ArgumentException("Appointment hour not found");

            appointmentHour.IsActive = dto.IsActive;
            appointmentHour.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new AppointmentHourDto
            {
                Id = appointmentHour.Id.ToString(),
                DoctorId = appointmentHour.DoctorId.ToString(),
                Hour = appointmentHour.Hour.ToString(@"hh\:mm"),
                DayOfWeek = appointmentHour.DayOfWeek.ToString(),
                IsActive = appointmentHour.IsActive,
                CreatedAt = appointmentHour.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                UpdatedAt = appointmentHour.UpdatedAt?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
            };
        }

        public async Task<bool> DeleteAppointmentHourAsync(int id)
        {
            var appointmentHour = await _context.AppointmentHours.FindAsync(id);
            if (appointmentHour == null)
                return false;

            _context.AppointmentHours.Remove(appointmentHour);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<AppointmentHourDto?> GetAppointmentHourByIdAsync(int id)
        {
            var appointmentHour = await _context.AppointmentHours.FindAsync(id);
            if (appointmentHour == null)
                return null;

            return new AppointmentHourDto
            {
                Id = appointmentHour.Id.ToString(),
                DoctorId = appointmentHour.DoctorId.ToString(),
                Hour = appointmentHour.Hour.ToString(@"hh\:mm"),
                DayOfWeek = appointmentHour.DayOfWeek.ToString(),
                IsActive = appointmentHour.IsActive,
                CreatedAt = appointmentHour.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                UpdatedAt = appointmentHour.UpdatedAt?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
            };
        }

        public async Task<bool> AppointmentHourExistsAsync(int doctorId, TimeSpan hour, DayOfWeek dayOfWeek)
        {
            return await _context.AppointmentHours
                .AnyAsync(ah => ah.DoctorId == doctorId && ah.Hour == hour && ah.DayOfWeek == dayOfWeek);
        }

        public async Task CreateDefaultAppointmentHoursForDoctorAsync(int doctorId)
        {
            // Check if the doctor already has appointment hours
            var existingHours = await _context.AppointmentHours
                .Where(ah => ah.DoctorId == doctorId)
                .AnyAsync();

            if (existingHours)
                return; // Doctor already has appointment hours

            var appointmentHours = new List<AppointmentHour>();

            // Monday to Friday (9 AM to 9 PM - every hour)
            for (int day = 1; day <= 5; day++) // 1=Monday, 5=Friday
            {
                for (int hour = 9; hour <= 21; hour++)
                {
                    appointmentHours.Add(new AppointmentHour
                    {
                        DoctorId = doctorId,
                        DayOfWeek = (DayOfWeek)day,
                        Hour = new TimeSpan(hour, 0, 0),
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            // Saturday - reduced hours (9 AM to 1 PM)
            for (int hour = 9; hour <= 13; hour++)
            {
                appointmentHours.Add(new AppointmentHour
                {
                    DoctorId = doctorId,
                    DayOfWeek = DayOfWeek.Saturday,
                    Hour = new TimeSpan(hour, 0, 0),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }

            // No hours for Sunday (day off)

            if (appointmentHours.Any())
            {
                _context.AppointmentHours.AddRange(appointmentHours);
                await _context.SaveChangesAsync();
            }
        }
    }
}