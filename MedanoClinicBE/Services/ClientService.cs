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
        private readonly IJobService _jobService;

        public ClientService(
            IDoctorRepository doctorRepository, 
            IAppointmentRepository appointmentRepository, 
            IReviewRepository reviewRepository,
            IJobService jobService)
        {
            _doctorRepository = doctorRepository;
            _appointmentRepository = appointmentRepository;
            _reviewRepository = reviewRepository;
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
    }
}