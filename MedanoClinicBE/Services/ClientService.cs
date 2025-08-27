using MedanoClinicBE.DTOs;
using MedanoClinicBE.Repositories.Interfaces;
using MedanoClinicBE.Services.Interfaces;

namespace MedanoClinicBE.Services
{
    public class ClientService : IClientService
    {
        private readonly IDoctorRepository _doctorRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IReviewRepository _reviewRepository;
        private readonly IAppointmentHourRepository _appointmentHourRepository;
        private readonly IJobService _jobService;

        public ClientService(
            IDoctorRepository doctorRepository, 
            IAppointmentRepository appointmentRepository, 
            IReviewRepository reviewRepository,
            IAppointmentHourRepository appointmentHourRepository,
            IJobService jobService)
        {
            _doctorRepository = doctorRepository;
            _appointmentRepository = appointmentRepository;
            _reviewRepository = reviewRepository;
            _appointmentHourRepository = appointmentHourRepository;
            _jobService = jobService;
        }

        public async Task<List<DoctorDto>> GetDoctorsAsync()
        {
            return await _doctorRepository.GetAllActiveDoctorsAsync();
        }

        public async Task<List<AppointmentResponseDto>> GetClientAppointmentsAsync(string clientId)
        {
            return await _appointmentRepository.GetClientAppointmentsAsync(clientId);
        }

        public async Task<AppointmentResponseDto> CreateAppointmentAsync(CreateAppointmentDto dto, string clientId)
        {
            // Create the appointment first
            var appointment = await _appointmentRepository.CreateAppointmentAsync(dto, clientId);
            
            // Use Hangfire to send appointment creation emails asynchronously
            _jobService.SendAppointmentCreatedEmailJob(appointment);
            
            // Schedule reminder (1 hour before appointment)
            _jobService.ScheduleAppointmentReminder(appointment);
            
            return appointment;
        }

        public async Task<ReviewDto> CreateReviewAsync(CreateReviewDto dto, string clientId)
        {
            return await _reviewRepository.CreateReviewAsync(dto, clientId);
        }

        public async Task<List<ReviewDto>> GetAllReviewsAsync()
        {
            return await _reviewRepository.GetAllReviewsAsync();
        }

        // Appointment Hours Management
        public async Task<List<AppointmentHourDto>> GetDoctorAppointmentHoursAsync(string doctorId)
        {
            var id = int.Parse(doctorId);
            return await _appointmentHourRepository.GetDoctorAppointmentHoursAsync(id);
        }

        public async Task<List<AppointmentHourDto>> GetDoctorAppointmentHoursByDayAsync(string doctorId, string dayOfWeek)
        {
            var id = int.Parse(doctorId);
            var day = Enum.Parse<DayOfWeek>(dayOfWeek);
            return await _appointmentHourRepository.GetDoctorAppointmentHoursByDayAsync(id, day);
        }

        public async Task<List<DoctorAppointmentHoursDto>> GetAllDoctorsAppointmentHoursAsync()
        {
            return await _appointmentHourRepository.GetAllDoctorsAppointmentHoursAsync();
        }
    }
}