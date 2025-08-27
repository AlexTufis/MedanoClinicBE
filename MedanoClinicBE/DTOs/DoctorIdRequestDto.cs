using System.ComponentModel.DataAnnotations;

namespace MedanoClinicBE.DTOs
{
    public class DoctorIdRequestDto
    {
        [Required]
        public string DoctorUserId { get; set; }
    }
}