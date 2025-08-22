using System.ComponentModel.DataAnnotations;

namespace MedanoClinicBE.DTOs
{
    public class CreateAppointmentDto
    {
        [Required]
        public string DoctorId { get; set; } // Using string for frontend compatibility
        
        [Required]
        public string AppointmentDate { get; set; } // Using string for frontend compatibility
        
        [Required]
        public string AppointmentTime { get; set; } // Using string for frontend compatibility
        
        [Required]
        public string Reason { get; set; }
        
        public string? Notes { get; set; }
    }
}