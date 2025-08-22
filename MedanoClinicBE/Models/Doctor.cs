using System.ComponentModel.DataAnnotations;

namespace MedanoClinicBE.Models
{
    public class Doctor
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        
        [Required]
        public string Specialization { get; set; }
        
        public string? Phone { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}