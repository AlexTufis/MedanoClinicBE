using System.ComponentModel.DataAnnotations;

namespace MedanoClinicBE.DTOs
{
    public class ReviewDto
    {
        public string Id { get; set; } // String format for frontend compatibility
        public string DoctorId { get; set; } // String format for frontend compatibility
        public string ClientId { get; set; }
        public string ClientName { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public string CreatedAt { get; set; } // String format for frontend compatibility
        public string AppointmentId { get; set; } // String format for frontend compatibility
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