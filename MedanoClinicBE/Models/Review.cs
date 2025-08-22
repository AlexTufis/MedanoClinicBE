using System.ComponentModel.DataAnnotations;

namespace MedanoClinicBE.Models
{
    public class Review
    {
        public int Id { get; set; }
        
        [Required]
        public string ClientId { get; set; }
        public ApplicationUser Client { get; set; }
        
        [Required]
        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; }
        
        [Required]
        public int AppointmentId { get; set; }
        public Appointment Appointment { get; set; }
        
        [Range(1, 5)]
        public int Rating { get; set; }
        
        public string? Comment { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}