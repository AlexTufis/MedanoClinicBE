using System.ComponentModel.DataAnnotations;

namespace MedanoClinicBE.DTOs
{
    public class ReviewDto
    {
        public string Id { get; set; } // String format for frontend compatibility
        public string DoctorId { get; set; } // String format for frontend compatibility
        public string DoctorName { get; set; } // NEW: Doctor's full name
        public string ClientId { get; set; }
        public string ClientName { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public string CreatedAt { get; set; } // String format for frontend compatibility
        public string AppointmentId { get; set; } // String format for frontend compatibility
        public string AppointmentDate { get; set; } // NEW: Appointment date in string format
    }

    public class CreateReviewDto
    {
        [Required]
        public int DoctorId { get; set; }
        
        [Required]
        public int AppointmentId { get; set; }
        
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }
        
        public string? Comment { get; set; }
    }
}