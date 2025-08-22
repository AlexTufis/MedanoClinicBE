using Microsoft.AspNetCore.Identity;

namespace MedanoClinicBE.Models
{
    public enum Gender
    {
        Male,
        Female,
        Other
    }

    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName { get; set; }

        public DateTime? DateOfBirth { get; set; }
        public Gender? Gender { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
