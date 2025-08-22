namespace MedanoClinicBE.DTOs
{
    public class AdminDashboardDto
    {
        public int TotalUsers { get; set; }
        public int ClientUsers { get; set; }
        public int AdminUsers { get; set; }
        public int DoctorUsers { get; set; }
        public int NewUsersThisMonth { get; set; }
        public int TotalAppointments { get; set; }
        public int TodayAppointments { get; set; }
        public int WeeklyAppointments { get; set; }
        public AppointmentsByStatus AppointmentsByStatus { get; set; }
    }

    public class AppointmentsByStatus
    {
        public int Scheduled { get; set; }
        public int Completed { get; set; }
        public int Cancelled { get; set; }
        public int NoShow { get; set; }
    }
}