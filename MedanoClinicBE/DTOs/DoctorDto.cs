namespace MedanoClinicBE.DTOs
{
    public class DoctorDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Specialization { get; set; }
        public string Email { get; set; }
        public string? Phone { get; set; }
        public bool IsActive { get; set; }
        
        // Review Information
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
    }
}