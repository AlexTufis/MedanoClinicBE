namespace MedanoClinicBE.DTOs
{
    public class AuthResponseDto
    {
        public string Email { get; set; }
        public string Role { get; set; }
        public string Token { get; set; }
        public DateTime Expiry { get; set; }
    }
}
