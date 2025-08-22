namespace MedanoClinicBE.DTOs
{
    public class TimeSlotDto
    {
        public string Time { get; set; } // Format: "HH:MM"
        public bool Available { get; set; }
        public int DoctorId { get; set; }
    }

    public class DoctorScheduleDto
    {
        public int DoctorId { get; set; }
        public DateTime Date { get; set; } // Format: "YYYY-MM-DD"
        public List<TimeSlotDto> TimeSlots { get; set; } = new List<TimeSlotDto>();
    }
}