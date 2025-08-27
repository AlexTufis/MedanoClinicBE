using System.ComponentModel.DataAnnotations;

namespace MedanoClinicBE.DTOs
{
    public class UpdateUserRoleDto
    {
        [Required]
        public string UserId { get; set; }
        
        [Required]
        public string RoleName { get; set; }
        
        // Required when changing role to Doctor
        public string? Specialization { get; set; }
        
        // Optional when changing role to Doctor
        public string? Phone { get; set; }
    }
}