using System.ComponentModel.DataAnnotations;

namespace MedanoClinicBE.DTOs
{
    public class AppointmentHourDto
    {
        public string Id { get; set; } // String format for frontend compatibility
        public string DoctorId { get; set; } // String format for frontend compatibility
        public string Hour { get; set; } // String format (HH:mm) for frontend compatibility
        public string DayOfWeek { get; set; } // String format for frontend compatibility
        public bool IsActive { get; set; }
        public string CreatedAt { get; set; } // String format for frontend compatibility
        public string? UpdatedAt { get; set; } // String format for frontend compatibility
    }

    public class CreateAppointmentHourDto
    {
        [Required]
        public string DoctorId { get; set; } // String format for frontend compatibility
        
        [Required]
        public string Hour { get; set; } // String format (HH:mm) for frontend compatibility
        
        [Required]
        public string DayOfWeek { get; set; } // String format for frontend compatibility
        
        public bool IsActive { get; set; } = true;
    }

    public class UpdateAppointmentHourDto
    {
        public bool IsActive { get; set; }
    }

    public class DoctorAppointmentHoursDto
    {
        public string DoctorId { get; set; }
        public string DoctorName { get; set; }
        public string DoctorSpecialization { get; set; }
        public List<AppointmentHourDto> AppointmentHours { get; set; } = new();
    }
}