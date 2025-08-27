using MedanoClinicBE.Data;
using MedanoClinicBE.DTOs;
using MedanoClinicBE.Models;
using MedanoClinicBE.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MedanoClinicBE.Repositories
{
    public class DoctorRepository : IDoctorRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DoctorRepository(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<List<DoctorDto>> GetAllActiveDoctorsAsync()
        {
            // Get all active doctors first
            var doctors = await _context.Doctors
                .Include(d => d.User)
                .Where(d => d.IsActive)
                .ToListAsync();

            // Get all review statistics in one query
            var reviewStats = await _context.Reviews
                .GroupBy(r => r.DoctorId)
                .Select(g => new
                {
                    DoctorId = g.Key,
                    AverageRating = g.Average(r => (double)r.Rating),
                    TotalReviews = g.Count()
                })
                .ToListAsync();

            // Create a dictionary for fast lookup
            var reviewLookup = reviewStats.ToDictionary(rs => rs.DoctorId, rs => new { rs.AverageRating, rs.TotalReviews });

            var doctorDtos = new List<DoctorDto>();

            foreach (var doctor in doctors)
            {
                // Verify the user has the Doctor role
                var hasRole = await _userManager.IsInRoleAsync(doctor.User, "Doctor");
                if (hasRole)
                {
                    // Look up review stats for this doctor
                    var hasReviews = reviewLookup.TryGetValue(doctor.Id, out var stats);
                    var averageRating = hasReviews ? stats.AverageRating : 0.0;
                    var totalReviews = hasReviews ? stats.TotalReviews : 0;

                    doctorDtos.Add(new DoctorDto
                    {
                        Id = doctor.Id,
                        FirstName = doctor.User.FirstName,
                        LastName = doctor.User.LastName,
                        Specialization = doctor.Specialization,
                        Email = doctor.User.Email,
                        Phone = doctor.Phone,
                        IsActive = doctor.IsActive,
                        AverageRating = Math.Round(averageRating, 1), // Round to 1 decimal place
                        TotalReviews = totalReviews
                    });
                }
            }

            return doctorDtos;
        }

        public async Task<Doctor?> GetDoctorByUserIdAsync(string userId)
        {
            return await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.UserId == userId && d.IsActive);
        }

        public async Task<Doctor> CreateDoctorAsync(string userId, string specialization, string? phone = null)
        {
            var doctor = new Doctor
            {
                UserId = userId,
                Specialization = specialization,
                Phone = phone,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            // Return the created doctor with user information
            return await _context.Doctors
                .Include(d => d.User)
                .FirstAsync(d => d.Id == doctor.Id);
        }
    }
}