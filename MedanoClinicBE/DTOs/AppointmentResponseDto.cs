using MedanoClinicBE.Models;

namespace MedanoClinicBE.DTOs
{
    public class AppointmentResponseDto
    {
        public string Id { get; set; } // String format for frontend compatibility
        public string ClientId { get; set; }
        public string ClientName { get; set; }
        public string DoctorId { get; set; } // String format for frontend compatibility
        public string DoctorName { get; set; }
        public string DoctorSpecialization { get; set; }
        public string AppointmentDate { get; set; } // String format for frontend compatibility
        public string AppointmentTime { get; set; } // String format for frontend compatibility
        public string Status { get; set; } // String format for frontend compatibility
        public string Reason { get; set; }
        public string? Notes { get; set; }
        public string CreatedAt { get; set; } // String format for frontend compatibility
    }
}