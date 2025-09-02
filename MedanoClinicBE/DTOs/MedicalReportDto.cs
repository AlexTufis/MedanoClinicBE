using System.ComponentModel.DataAnnotations;

namespace MedanoClinicBE.DTOs
{
    public class CreateMedicalReportDto
    {
        [Required]
        public string AppointmentId { get; set; } // String for frontend compatibility
        
        public string? Antecedente { get; set; }    // Medical history
        
        public string? Simptome { get; set; }       // Symptoms
        
        public string? Clinice { get; set; }        // Clinical findings
        
        public string? Paraclinice { get; set; }    // Paraclinical findings
        
        public string? Diagnostic { get; set; }     // Diagnosis
        
        public string? Recomandari { get; set; }    // Recommendations
    }

    public class UpdateMedicalReportDto
    {
        public string? Antecedente { get; set; }    // Medical history
        
        public string? Simptome { get; set; }       // Symptoms
        
        public string? Clinice { get; set; }        // Clinical findings
        
        public string? Paraclinice { get; set; }    // Paraclinical findings
        
        public string? Diagnostic { get; set; }     // Diagnosis
        
        public string? Recomandari { get; set; }    // Recommendations
    }

    public class MedicalReportDto
    {
        public string Id { get; set; }              // String format for frontend compatibility
        
        public string AppointmentId { get; set; }   // String format for frontend compatibility
        
        public string DoctorId { get; set; }        // String format for frontend compatibility
        
        public string DoctorName { get; set; }
        
        public string DoctorSpecialization { get; set; }
        
        public string PatientId { get; set; }
        
        public string PatientName { get; set; }
        
        public string AppointmentDate { get; set; } // String format for frontend compatibility
        
        public string AppointmentTime { get; set; } // String format for frontend compatibility
        
        public string? Antecedente { get; set; }    // Medical history
        
        public string? Simptome { get; set; }       // Symptoms
        
        public string? Clinice { get; set; }        // Clinical findings
        
        public string? Paraclinice { get; set; }    // Paraclinical findings
        
        public string? Diagnostic { get; set; }     // Diagnosis
        
        public string? Recomandari { get; set; }    // Recommendations
        
        public string CreatedAt { get; set; }       // String format for frontend compatibility
        
        public string? UpdatedAt { get; set; }      // String format for frontend compatibility
    }
}