using System.ComponentModel.DataAnnotations;

namespace MedanoClinicBE.Models
{
    public class MedicalReport
    {
        public int Id { get; set; }
        
        [Required]
        public int AppointmentId { get; set; }
        public Appointment Appointment { get; set; }
        
        [Required]
        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; }
        
        [Required]
        public string PatientId { get; set; }
        public ApplicationUser Patient { get; set; }
        
        // Romanian medical report fields based on the image
        public string? Antecedente { get; set; }  // Medical history
        
        public string? Simptome { get; set; }     // Symptoms
        
        public string? Clinice { get; set; }      // Clinical findings
        
        public string? Paraclinice { get; set; }  // Paraclinical findings
        
        public string? Diagnostic { get; set; }   // Diagnosis
        
        public string? Recomandari { get; set; }  // Recommendations
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
    }
}