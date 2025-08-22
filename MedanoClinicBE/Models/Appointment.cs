using System.ComponentModel.DataAnnotations;

namespace MedanoClinicBE.Models
{
    public enum AppointmentStatus
    {
        Scheduled,
        InProgress,
        Completed,
        Cancelled,
        NoShow
    }

    public class Appointment
    {
        public int Id { get; set; }
        
        [Required]
        public string PatientId { get; set; }
        public ApplicationUser Patient { get; set; }
        
        [Required]
        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; }
        
        [Required]
        public DateTime AppointmentDate { get; set; }
        
        [Required]
        public TimeSpan AppointmentTime { get; set; }
        
        [Required]
        public string Reason { get; set; }
        
        public string? Notes { get; set; }
        
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? CompletedAt { get; set; }
    }
}