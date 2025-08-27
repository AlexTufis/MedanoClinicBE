using System.ComponentModel.DataAnnotations;

namespace MedanoClinicBE.Models
{
    public class AppointmentHour
    {
        public int Id { get; set; }
        
        [Required]
        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; }
        
        [Required]
        public TimeSpan Hour { get; set; }
        
        [Required]
        public DayOfWeek DayOfWeek { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
    }
}