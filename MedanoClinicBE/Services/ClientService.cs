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

        public ClientService(IDoctorRepository doctorRepository, IAppointmentRepository appointmentRepository, IReviewRepository reviewRepository)
        {
            _doctorRepository = doctorRepository;
            _appointmentRepository = appointmentRepository;
            _reviewRepository = reviewRepository;
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
            return await _appointmentRepository.CreateAppointmentAsync(dto, clientId);
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