using MedanoClinicBE.Models;

namespace MedanoClinicBE.DTOs
{
    public class UserDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName { get; set; }
        public string? DateOfBirth { get; set; } // String format for frontend compatibility
        public int? Gender { get; set; } // Number format for frontend compatibility
        public string Role { get; set; }
        public string CreatedAt { get; set; } // String format for frontend compatibility
        public bool IsActive { get; set; }
        public List<MedicalReportDto> MedicalReports { get; set; } = new List<MedicalReportDto>();
    }
}