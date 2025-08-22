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
            var doctors = await _context.Doctors
                .Include(d => d.User)
                .Where(d => d.IsActive)
                .ToListAsync();

            var doctorDtos = new List<DoctorDto>();

            foreach (var doctor in doctors)
            {
                // Verify the user has the Doctor role
                var hasRole = await _userManager.IsInRoleAsync(doctor.User, "Doctor");
                if (hasRole)
                {
                    doctorDtos.Add(new DoctorDto
                    {
                        Id = doctor.Id,
                        FirstName = doctor.User.FirstName,
                        LastName = doctor.User.LastName,
                        Specialization = doctor.Specialization,
                        Email = doctor.User.Email,
                        Phone = doctor.Phone,
                        IsActive = doctor.IsActive
                    });
                }
            }

            return doctorDtos;
        }
    }
}