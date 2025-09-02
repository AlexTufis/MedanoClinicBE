using System.ComponentModel.DataAnnotations;

namespace MedanoClinicBE.DTOs
{
    public class UpdateAppointmentStatusDto
    {
        [Required]
        public string Status { get; set; } // "scheduled", "completed", "cancelled", "no-show", "in-progress"
        
        public string? AdminNotes { get; set; } // Optional notes from admin explaining the status change
    }
}