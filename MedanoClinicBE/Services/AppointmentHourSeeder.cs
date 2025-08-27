using MedanoClinicBE.Data;
using MedanoClinicBE.Models;
using Microsoft.EntityFrameworkCore;

namespace MedanoClinicBE.Services
{
    public static class AppointmentHourSeeder
    {
        public static async Task SeedDefaultAppointmentHoursAsync(ApplicationDbContext context)
        {
            // Check if appointment hours already exist
            if (await context.AppointmentHours.AnyAsync())
                return;

            // Get all active doctors
            var doctors = await context.Doctors
                .Where(d => d.IsActive)
                .ToListAsync();

            if (!doctors.Any())
                return;

            var appointmentHours = new List<AppointmentHour>();

            // Create default appointment hours for each doctor (9 AM to 9 PM, Monday to Friday)
            foreach (var doctor in doctors)
            {
                // Monday to Friday
                for (int day = 1; day <= 5; day++) // 1=Monday, 5=Friday
                {
                    // 9 AM to 9 PM (every hour)
                    for (int hour = 9; hour <= 21; hour++)
                    {
                        appointmentHours.Add(new AppointmentHour
                        {
                            DoctorId = doctor.Id,
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
                        DoctorId = doctor.Id,
                        DayOfWeek = DayOfWeek.Saturday,
                        Hour = new TimeSpan(hour, 0, 0),
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            if (appointmentHours.Any())
            {
                context.AppointmentHours.AddRange(appointmentHours);
                await context.SaveChangesAsync();
            }
        }
    }
}