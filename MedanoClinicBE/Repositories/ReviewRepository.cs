using MedanoClinicBE.Data;
using MedanoClinicBE.DTOs;
using MedanoClinicBE.Models;
using MedanoClinicBE.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MedanoClinicBE.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly ApplicationDbContext _context;

        public ReviewRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ReviewDto> CreateReviewAsync(CreateReviewDto dto, string clientId)
        {
            // Check if the appointment exists and belongs to the client
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == dto.AppointmentId && a.PatientId == clientId);

            if (appointment == null)
            {
                throw new ArgumentException("Appointment not found or does not belong to the current user.");
            }

            // Check if the appointment is completed
            if (appointment.Status != AppointmentStatus.Completed)
            {
                throw new InvalidOperationException("Reviews can only be created for completed appointments.");
            }

            // Check if a review already exists for this appointment
            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.AppointmentId == dto.AppointmentId);

            if (existingReview != null)
            {
                throw new InvalidOperationException("A review already exists for this appointment.");
            }

            var review = new Review
            {
                ClientId = clientId,
                DoctorId = dto.DoctorId,
                AppointmentId = dto.AppointmentId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            // Fetch the review with related data including appointment
            var createdReview = await _context.Reviews
                .Include(r => r.Client)
                .Include(r => r.Doctor)
                .ThenInclude(d => d.User)
                .Include(r => r.Appointment) // Include appointment data
                .FirstOrDefaultAsync(r => r.Id == review.Id);

            return new ReviewDto
            {
                Id = createdReview.Id.ToString(),
                DoctorId = createdReview.DoctorId.ToString(),
                DoctorName = $"{createdReview.Doctor.User.FirstName} {createdReview.Doctor.User.LastName}",
                ClientId = createdReview.ClientId,
                ClientName = $"{createdReview.Client.FirstName} {createdReview.Client.LastName}",
                Rating = createdReview.Rating,
                Comment = createdReview.Comment,
                CreatedAt = createdReview.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                AppointmentId = createdReview.AppointmentId.ToString(),
                AppointmentDate = createdReview.Appointment.AppointmentDate.ToString("yyyy-MM-dd")
            };
        }

        public async Task<List<ReviewDto>> GetAllReviewsAsync()
        {
            var reviews = await _context.Reviews
                .Include(r => r.Client)
                .Include(r => r.Doctor)
                .ThenInclude(d => d.User)
                .Include(r => r.Appointment) // Include appointment data
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var reviewDtos = new List<ReviewDto>();

            foreach (var review in reviews)
            {
                reviewDtos.Add(new ReviewDto
                {
                    Id = review.Id.ToString(),
                    DoctorId = review.DoctorId.ToString(),
                    DoctorName = $"{review.Doctor.User.FirstName} {review.Doctor.User.LastName}",
                    ClientId = review.ClientId,
                    ClientName = $"{review.Client.FirstName} {review.Client.LastName}",
                    Rating = review.Rating,
                    Comment = review.Comment,
                    CreatedAt = review.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    AppointmentId = review.AppointmentId.ToString(),
                    AppointmentDate = review.Appointment.AppointmentDate.ToString("yyyy-MM-dd")
                });
            }

            return reviewDtos;
        }

        public async Task<List<ReviewDto>> GetDoctorReviewsAsync(int doctorId)
        {
            var reviews = await _context.Reviews
                .Include(r => r.Client)
                .Include(r => r.Doctor)
                .ThenInclude(d => d.User)
                .Include(r => r.Appointment) // Include appointment data
                .Where(r => r.DoctorId == doctorId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var reviewDtos = new List<ReviewDto>();

            foreach (var review in reviews)
            {
                reviewDtos.Add(new ReviewDto
                {
                    Id = review.Id.ToString(),
                    DoctorId = review.DoctorId.ToString(),
                    DoctorName = $"{review.Doctor.User.FirstName} {review.Doctor.User.LastName}",
                    ClientId = review.ClientId,
                    ClientName = $"{review.Client.FirstName} {review.Client.LastName}",
                    Rating = review.Rating,
                    Comment = review.Comment,
                    CreatedAt = review.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    AppointmentId = review.AppointmentId.ToString(),
                    AppointmentDate = review.Appointment.AppointmentDate.ToString("yyyy-MM-dd")
                });
            }

            return reviewDtos;
        }
    }
}