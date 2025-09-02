using MedanoClinicBE.Data;
using MedanoClinicBE.DTOs;
using MedanoClinicBE.Models;
using MedanoClinicBE.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MedanoClinicBE.Repositories
{
    public class MedicalReportRepository : IMedicalReportRepository
    {
        private readonly ApplicationDbContext _context;

        public MedicalReportRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<MedicalReportDto> CreateMedicalReportAsync(CreateMedicalReportDto dto, string doctorUserId)
        {
            var appointmentId = int.Parse(dto.AppointmentId);

            // Get appointment and verify it exists and belongs to the doctor
            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
            {
                throw new ArgumentException("Appointment not found.");
            }

            if (appointment.Doctor.UserId != doctorUserId)
            {
                throw new UnauthorizedAccessException("You can only create medical reports for your own appointments.");
            }

            if (appointment.Status != AppointmentStatus.Completed)
            {
                throw new InvalidOperationException("Medical reports can only be created for completed appointments.");
            }

            // Check if medical report already exists for this appointment
            var existingReport = await _context.MedicalReports
                .FirstOrDefaultAsync(mr => mr.AppointmentId == appointmentId);

            if (existingReport != null)
            {
                throw new InvalidOperationException("A medical report already exists for this appointment.");
            }

            var medicalReport = new MedicalReport
            {
                AppointmentId = appointmentId,
                DoctorId = appointment.DoctorId,
                PatientId = appointment.PatientId,
                Antecedente = dto.Antecedente,
                Simptome = dto.Simptome,
                Clinice = dto.Clinice,
                Paraclinice = dto.Paraclinice,
                Diagnostic = dto.Diagnostic,
                Recomandari = dto.Recomandari,
                CreatedAt = DateTime.UtcNow
            };

            _context.MedicalReports.Add(medicalReport);
            await _context.SaveChangesAsync();

            // Fetch the created report with related data
            var createdReport = await _context.MedicalReports
                .Include(mr => mr.Appointment)
                .Include(mr => mr.Doctor)
                .ThenInclude(d => d.User)
                .Include(mr => mr.Patient)
                .FirstOrDefaultAsync(mr => mr.Id == medicalReport.Id);

            return MapToDto(createdReport!);
        }

        public async Task<MedicalReportDto?> UpdateMedicalReportAsync(string reportId, UpdateMedicalReportDto dto, string doctorUserId)
        {
            var id = int.Parse(reportId);

            var medicalReport = await _context.MedicalReports
                .Include(mr => mr.Appointment)
                .Include(mr => mr.Doctor)
                .ThenInclude(d => d.User)
                .Include(mr => mr.Patient)
                .FirstOrDefaultAsync(mr => mr.Id == id);

            if (medicalReport == null)
            {
                return null;
            }

            if (medicalReport.Doctor.UserId != doctorUserId)
            {
                throw new UnauthorizedAccessException("You can only update your own medical reports.");
            }

            // Update fields
            medicalReport.Antecedente = dto.Antecedente;
            medicalReport.Simptome = dto.Simptome;
            medicalReport.Clinice = dto.Clinice;
            medicalReport.Paraclinice = dto.Paraclinice;
            medicalReport.Diagnostic = dto.Diagnostic;
            medicalReport.Recomandari = dto.Recomandari;
            medicalReport.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToDto(medicalReport);
        }

        public async Task<MedicalReportDto?> GetMedicalReportByIdAsync(string reportId)
        {
            var id = int.Parse(reportId);

            var medicalReport = await _context.MedicalReports
                .Include(mr => mr.Appointment)
                .Include(mr => mr.Doctor)
                .ThenInclude(d => d.User)
                .Include(mr => mr.Patient)
                .FirstOrDefaultAsync(mr => mr.Id == id);

            return medicalReport == null ? null : MapToDto(medicalReport);
        }

        public async Task<MedicalReportDto?> GetMedicalReportByAppointmentIdAsync(string appointmentId)
        {
            var id = int.Parse(appointmentId);

            var medicalReport = await _context.MedicalReports
                .Include(mr => mr.Appointment)
                .Include(mr => mr.Doctor)
                .ThenInclude(d => d.User)
                .Include(mr => mr.Patient)
                .FirstOrDefaultAsync(mr => mr.AppointmentId == id);

            return medicalReport == null ? null : MapToDto(medicalReport);
        }

        public async Task<List<MedicalReportDto>> GetMedicalReportsByDoctorAsync(string doctorUserId)
        {
            var medicalReports = await _context.MedicalReports
                .Include(mr => mr.Appointment)
                .Include(mr => mr.Doctor)
                .ThenInclude(d => d.User)
                .Include(mr => mr.Patient)
                .Where(mr => mr.Doctor.UserId == doctorUserId)
                .OrderByDescending(mr => mr.CreatedAt)
                .ToListAsync();

            return medicalReports.Select(MapToDto).ToList();
        }

        public async Task<List<MedicalReportDto>> GetMedicalReportsByPatientAsync(string patientUserId)
        {
            var medicalReports = await _context.MedicalReports
                .Include(mr => mr.Appointment)
                .Include(mr => mr.Doctor)
                .ThenInclude(d => d.User)
                .Include(mr => mr.Patient)
                .Where(mr => mr.PatientId == patientUserId)
                .OrderByDescending(mr => mr.CreatedAt)
                .ToListAsync();

            return medicalReports.Select(MapToDto).ToList();
        }

        public async Task<bool> DeleteMedicalReportAsync(string reportId, string doctorUserId)
        {
            var id = int.Parse(reportId);

            var medicalReport = await _context.MedicalReports
                .Include(mr => mr.Doctor)
                .ThenInclude(d => d.User)
                .FirstOrDefaultAsync(mr => mr.Id == id);

            if (medicalReport == null)
            {
                return false;
            }

            if (medicalReport.Doctor.UserId != doctorUserId)
            {
                throw new UnauthorizedAccessException("You can only delete your own medical reports.");
            }

            _context.MedicalReports.Remove(medicalReport);
            await _context.SaveChangesAsync();

            return true;
        }

        private static MedicalReportDto MapToDto(MedicalReport medicalReport)
        {
            return new MedicalReportDto
            {
                Id = medicalReport.Id.ToString(),
                AppointmentId = medicalReport.AppointmentId.ToString(),
                DoctorId = medicalReport.DoctorId.ToString(),
                DoctorName = $"{medicalReport.Doctor.User.FirstName} {medicalReport.Doctor.User.LastName}",
                DoctorSpecialization = medicalReport.Doctor.Specialization,
                PatientId = medicalReport.PatientId,
                PatientName = $"{medicalReport.Patient.FirstName} {medicalReport.Patient.LastName}",
                AppointmentDate = medicalReport.Appointment.AppointmentDate.ToString("yyyy-MM-dd"),
                AppointmentTime = medicalReport.Appointment.AppointmentTime.ToString(@"hh\:mm"),
                Antecedente = medicalReport.Antecedente,
                Simptome = medicalReport.Simptome,
                Clinice = medicalReport.Clinice,
                Paraclinice = medicalReport.Paraclinice,
                Diagnostic = medicalReport.Diagnostic,
                Recomandari = medicalReport.Recomandari,
                CreatedAt = medicalReport.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                UpdatedAt = medicalReport.UpdatedAt?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
            };
        }
    }
}