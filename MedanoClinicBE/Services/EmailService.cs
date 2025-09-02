using MedanoClinicBE.DTOs;
using MedanoClinicBE.Models;
using MedanoClinicBE.Services.Interfaces;
using MedanoClinicBE.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace MedanoClinicBE.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public EmailService(
            IOptions<EmailSettings> emailSettings, 
            ILogger<EmailService> logger,
            UserManager<ApplicationUser> userManager)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<bool> SendEmailAsync(EmailNotificationDto emailDto)
        {
            try
            {
                // Log email settings for debugging
                _logger.LogInformation("Attempting to send email using SMTP settings: Host={SmtpHost}, Port={SmtpPort}, Username={SmtpUsername}, EnableSsl={EnableSsl}", 
                    _emailSettings.SmtpHost, _emailSettings.SmtpPort, _emailSettings.SmtpUsername, _emailSettings.EnableSsl);

                // Validate email address before attempting to send
                if (string.IsNullOrWhiteSpace(emailDto.ToEmail))
                {
                    _logger.LogWarning("Cannot send email: ToEmail is null or empty. Using fallback email.");
                    emailDto.ToEmail = "test@example.com"; // Set fallback email for testing
                }

                // Additional validation to ensure email format is reasonable
                if (!IsValidEmail(emailDto.ToEmail))
                {
                    _logger.LogWarning("Invalid email format: {Email}. Using fallback email.", emailDto.ToEmail);
                    emailDto.ToEmail = "test@example.com";
                }

                // Validate FromEmail and FromName settings
                var fromEmail = string.IsNullOrWhiteSpace(_emailSettings.FromEmail) 
                    ? "noreply@medanoclinic.com" 
                    : _emailSettings.FromEmail;
                    
                var fromName = string.IsNullOrWhiteSpace(_emailSettings.FromName) 
                    ? "MedanoClinic" 
                    : _emailSettings.FromName;

                _logger.LogInformation("Sending email from {FromEmail} ({FromName}) to {ToEmail} with subject: {Subject}", 
                    fromEmail, fromName, emailDto.ToEmail, emailDto.Subject);

                using var client = new SmtpClient(_emailSettings.SmtpHost, _emailSettings.SmtpPort)
                {
                    Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword),
                    EnableSsl = _emailSettings.EnableSsl
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = emailDto.Subject,
                    IsBodyHtml = false // Set to false initially
                };

                mailMessage.To.Add(new MailAddress(emailDto.ToEmail, emailDto.ToName ?? "Patient"));

                // Create proper multipart email with HTML and plain text alternatives
                if (!string.IsNullOrEmpty(emailDto.HtmlBody) && !string.IsNullOrEmpty(emailDto.PlainTextBody))
                {
                    // Create HTML view
                    var htmlView = AlternateView.CreateAlternateViewFromString(emailDto.HtmlBody, null, "text/html");
                    mailMessage.AlternateViews.Add(htmlView);

                    // Create plain text view
                    var plainView = AlternateView.CreateAlternateViewFromString(emailDto.PlainTextBody, null, "text/plain");
                    mailMessage.AlternateViews.Add(plainView);

                    // Don't set Body property when using AlternateViews
                }
                else if (!string.IsNullOrEmpty(emailDto.HtmlBody))
                {
                    // HTML only
                    mailMessage.Body = emailDto.HtmlBody;
                    mailMessage.IsBodyHtml = true;
                }
                else if (!string.IsNullOrEmpty(emailDto.PlainTextBody))
                {
                    // Plain text only
                    mailMessage.Body = emailDto.PlainTextBody;
                    mailMessage.IsBodyHtml = false;
                }

                _logger.LogInformation("Attempting SMTP connection and email send...");
                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("? Email sent successfully to {Email} with subject: {Subject}", emailDto.ToEmail, emailDto.Subject);
                return true;
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError(smtpEx, "? SMTP Error sending email to {Email} with subject: {Subject}. StatusCode: {StatusCode}", 
                    emailDto.ToEmail, emailDto.Subject, smtpEx.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? General error sending email to {Email} with subject: {Subject}", emailDto.ToEmail, emailDto.Subject);
                return false;
            }
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Simple email validation - just check if it can create a MailAddress
                var mailAddress = new MailAddress(email);
                return mailAddress.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SendAppointmentCreatedEmailAsync(AppointmentResponseDto appointment)
        {
            var emailDto = new EmailNotificationDto
            {
                ToEmail = await GetPatientEmailAsync(appointment.ClientId),
                ToName = appointment.ClientName,
                Subject = "Confirmare Programare - MedanoClinic",
                Type = NotificationType.AppointmentCreated,
                AppointmentId = appointment.Id,
                HtmlBody = GenerateAppointmentCreatedHtml(appointment),
                PlainTextBody = GenerateAppointmentCreatedPlainText(appointment)
            };

            return await SendEmailAsync(emailDto);
        }

        public async Task<bool> SendAppointmentModifiedEmailAsync(AppointmentResponseDto appointment)
        {
            var emailDto = new EmailNotificationDto
            {
                ToEmail = await GetPatientEmailAsync(appointment.ClientId),
                ToName = appointment.ClientName,
                Subject = "Programare Actualizată - MedanoClinic",
                Type = NotificationType.AppointmentModified,
                AppointmentId = appointment.Id,
                HtmlBody = GenerateAppointmentModifiedHtml(appointment),
                PlainTextBody = GenerateAppointmentModifiedPlainText(appointment)
            };

            return await SendEmailAsync(emailDto);
        }

        public async Task<bool> SendAppointmentReminderEmailAsync(AppointmentResponseDto appointment)
        {
            var emailDto = new EmailNotificationDto
            {
                ToEmail = await GetPatientEmailAsync(appointment.ClientId),
                ToName = appointment.ClientName,
                Subject = "Memento Programare - MedanoClinic",
                Type = NotificationType.AppointmentReminder,
                AppointmentId = appointment.Id,
                HtmlBody = GenerateAppointmentReminderHtml(appointment),
                PlainTextBody = GenerateAppointmentReminderPlainText(appointment)
            };

            return await SendEmailAsync(emailDto);
        }

        public async Task<bool> SendAppointmentCancelledEmailAsync(AppointmentResponseDto appointment)
        {
            var emailDto = new EmailNotificationDto
            {
                ToEmail = await GetPatientEmailAsync(appointment.ClientId),
                ToName = appointment.ClientName,
                Subject = "Programare Anulată - MedanoClinic",
                Type = NotificationType.AppointmentCancelled,
                AppointmentId = appointment.Id,
                HtmlBody = GenerateAppointmentCancelledHtml(appointment),
                PlainTextBody = GenerateAppointmentCancelledPlainText(appointment)
            };

            return await SendEmailAsync(emailDto);
        }

        private async Task<string> GetPatientEmailAsync(string clientId)
        {
            try
            {
                // Validate clientId first
                if (string.IsNullOrWhiteSpace(clientId))
                {
                    _logger.LogWarning("ClientId is null or empty. Using fallback email.");
                    return "noreply@medanoclinic.com";
                }

                // Get user by ID from AspNetUsers table using UserManager
                var user = await _userManager.FindByIdAsync(clientId);
                
                if (user != null && !string.IsNullOrWhiteSpace(user.Email))
                {
                    _logger.LogInformation("Retrieved email for client {ClientId}: {Email}", clientId, user.Email);
                    return user.Email.Trim(); // Trim any whitespace
                }
                
                _logger.LogWarning("User not found or email is empty for client {ClientId}", clientId);
                return "noreply@medanoclinic.com"; // Fallback email
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve email for client {ClientId}", clientId);
                return "noreply@medanoclinic.com"; // Fallback email
            }
        }

        private string GenerateAppointmentCreatedHtml(AppointmentResponseDto appointment)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html><head><meta charset=\"UTF-8\"><title>Confirmare Programare</title></head>");
            sb.AppendLine("<body style=\"font-family: Arial, sans-serif; line-height: 1.6; color: #333;\">");
            sb.AppendLine("<div style=\"max-width: 600px; margin: 0 auto; padding: 20px;\">");
            sb.AppendLine("<h2 style=\"color: #2c5aa0;\">Programare Confirmată</h2>");
            sb.AppendLine($"<p>Stimate/Stimată {appointment.ClientName},</p>");
            sb.AppendLine("<p>Programarea dumneavoastră a fost programată cu succes. Iată detaliile:</p>");
            sb.AppendLine("<div style=\"background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;\">");
            sb.AppendLine($"<p><strong>Doctor:</strong> {appointment.DoctorName} ({appointment.DoctorSpecialization})</p>");
            sb.AppendLine($"<p><strong>Data:</strong> {appointment.AppointmentDate}</p>");
            sb.AppendLine($"<p><strong>Ora:</strong> {appointment.AppointmentTime}</p>");
            sb.AppendLine($"<p><strong>Motivul:</strong> {appointment.Reason}</p>");
            if (!string.IsNullOrEmpty(appointment.Notes))
            {
                sb.AppendLine($"<p><strong>Note:</strong> {appointment.Notes}</p>");
            }
            sb.AppendLine("</div>");
            sb.AppendLine("<p>Vă rugăm să ajungeți cu 15 minute înainte de ora programării.</p>");
            sb.AppendLine("<p>Vă mulțumim că ați ales MedanoClinic!</p>");
            sb.AppendLine("<hr style=\"margin: 30px 0;\">");
            sb.AppendLine("<p style=\"font-size: 12px; color: #666;\">MedanoClinic - Sănătatea dumneavoastră, prioritatea noastră</p>");
            sb.AppendLine("</div></body></html>");
            return sb.ToString();
        }

        private string GenerateAppointmentCreatedPlainText(AppointmentResponseDto appointment)
        {
            var sb = new StringBuilder();
            sb.AppendLine("PROGRAMARE CONFIRMATĂ");
            sb.AppendLine("====================");
            sb.AppendLine($"Stimate/Stimată {appointment.ClientName},");
            sb.AppendLine();
            sb.AppendLine("Programarea dumneavoastră a fost programată cu succes. Iată detaliile:");
            sb.AppendLine();
            sb.AppendLine($"Doctor: {appointment.DoctorName} ({appointment.DoctorSpecialization})");
            sb.AppendLine($"Data: {appointment.AppointmentDate}");
            sb.AppendLine($"Ora: {appointment.AppointmentTime}");
            sb.AppendLine($"Motivul: {appointment.Reason}");
            if (!string.IsNullOrEmpty(appointment.Notes))
            {
                sb.AppendLine($"Note: {appointment.Notes}");
            }
            sb.AppendLine();
            sb.AppendLine("Vă rugăm să ajungeți cu 15 minute înainte de ora programării.");
            sb.AppendLine();
            sb.AppendLine("Vă mulțumim că ați ales MedanoClinic!");
            sb.AppendLine();
            sb.AppendLine("MedanoClinic - Sănătatea dumneavoastră, prioritatea noastră");
            return sb.ToString();
        }

        private string GenerateAppointmentModifiedHtml(AppointmentResponseDto appointment)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html><head><meta charset=\"UTF-8\"><title>Programare Actualizată</title></head>");
            sb.AppendLine("<body style=\"font-family: Arial, sans-serif; line-height: 1.6; color: #333;\">");
            sb.AppendLine("<div style=\"max-width: 600px; margin: 0 auto; padding: 20px;\">");
            sb.AppendLine("<h2 style=\"color: #f39c12;\">Programare Actualizată</h2>");
            sb.AppendLine($"<p>Stimate/Stimată {appointment.ClientName},</p>");
            sb.AppendLine("<p>Programarea dumneavoastră a fost actualizată. Iată detaliile actuale:</p>");
            sb.AppendLine("<div style=\"background-color: #fff3cd; padding: 15px; border-radius: 5px; margin: 20px 0; border-left: 4px solid #f39c12;\">");
            sb.AppendLine($"<p><strong>Doctor:</strong> {appointment.DoctorName} ({appointment.DoctorSpecialization})</p>");
            sb.AppendLine($"<p><strong>Data:</strong> {appointment.AppointmentDate}</p>");
            sb.AppendLine($"<p><strong>Ora:</strong> {appointment.AppointmentTime}</p>");
            sb.AppendLine($"<p><strong>Motivul:</strong> {appointment.Reason}</p>");
            if (!string.IsNullOrEmpty(appointment.Notes))
            {
                sb.AppendLine($"<p><strong>Note:</strong> {appointment.Notes}</p>");
            }
            sb.AppendLine("</div>");
            sb.AppendLine("<p>Vă rugăm să rețineți aceste modificări și să ajungeți cu 15 minute înainte de ora programării.</p>");
            sb.AppendLine("<p>Vă mulțumim că ați ales MedanoClinic!</p>");
            sb.AppendLine("<hr style=\"margin: 30px 0;\">");
            sb.AppendLine("<p style=\"font-size: 12px; color: #666;\">MedanoClinic - Sănătatea dumneavoastră, prioritatea noastră</p>");
            sb.AppendLine("</div></body></html>");
            return sb.ToString();
        }

        private string GenerateAppointmentModifiedPlainText(AppointmentResponseDto appointment)
        {
            var sb = new StringBuilder();
            sb.AppendLine("PROGRAMARE ACTUALIZATĂ");
            sb.AppendLine("======================");
            sb.AppendLine($"Stimate/Stimată {appointment.ClientName}, ");
            sb.AppendLine();
            sb.AppendLine("Programarea dumneavoastră a fost actualizată. Iată detaliile actuale:");
            sb.AppendLine();
            sb.AppendLine($"Doctor: {appointment.DoctorName} ({appointment.DoctorSpecialization})");
            sb.AppendLine($"Data: {appointment.AppointmentDate}");
            sb.AppendLine($"Ora: {appointment.AppointmentTime}");
            sb.AppendLine($"Motivul: {appointment.Reason}");
            if (!string.IsNullOrEmpty(appointment.Notes))
            {
                sb.AppendLine($"Note: {appointment.Notes}");
            }
            sb.AppendLine();
            sb.AppendLine("Vă rugăm să rețineți aceste modificări și să ajungeți cu 15 minute înainte de ora programării.");
            sb.AppendLine();
            sb.AppendLine("Vă mulțumim că ați ales MedanoClinic!");
            sb.AppendLine();
            sb.AppendLine("MedanoClinic - Sănătatea dumneavoastră, prioritatea noastră");
            return sb.ToString();
        }

        private string GenerateAppointmentReminderHtml(AppointmentResponseDto appointment)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html><head><meta charset=\"UTF-8\"><title>Memento Programare</title></head>");
            sb.AppendLine("<body style=\"font-family: Arial, sans-serif; line-height: 1.6; color: #333;\">");
            sb.AppendLine("<div style=\"max-width: 600px; margin: 0 auto; padding: 20px;\">");
            sb.AppendLine("<h2 style=\"color: #17a2b8;\">Memento Programare</h2>");
            sb.AppendLine($"<p>Stimate/Stimată {appointment.ClientName},</p>");
            sb.AppendLine("<p>Aceasta este o amintire prietenoasă că aveți o programare în 1 oră:</p>");
            sb.AppendLine("<div style=\"background-color: #d1ecf1; padding: 15px; border-radius: 5px; margin: 20px 0; border-left: 4px solid #17a2b8;\">");
            sb.AppendLine($"<p><strong>Doctor:</strong> {appointment.DoctorName} ({appointment.DoctorSpecialization})</p>");
            sb.AppendLine($"<p><strong>Data:</strong> {appointment.AppointmentDate}</p>");
            sb.AppendLine($"<p><strong>Ora:</strong> {appointment.AppointmentTime}</p>");
            sb.AppendLine($"<p><strong>Motivul:</strong> {appointment.Reason}</p>");
            sb.AppendLine("</div>");
            sb.AppendLine("<p><strong>Vă rugăm să vă amintiți să:</strong></p>");
            sb.AppendLine("<ul>");
            sb.AppendLine("<li>Ajungeți cu 15 minute mai devreme</li>");
            sb.AppendLine("<li>Aduceți actul de identitate și cardul de asigurări</li>");
            sb.AppendLine("<li>Aduceți documentele medicale relevante</li>");
            sb.AppendLine("</ul>");
            sb.AppendLine("<p>Vă mulțumim că ați ales MedanoClinic!</p>");
            sb.AppendLine("<hr style=\"margin: 30px 0;\">");
            sb.AppendLine("<p style=\"font-size: 12px; color: #666;\">MedanoClinic - Sănătatea dumneavoastră, prioritatea noastră</p>");
            sb.AppendLine("</div></body></html>");
            return sb.ToString();
        }

        private string GenerateAppointmentReminderPlainText(AppointmentResponseDto appointment)
        {
            var sb = new StringBuilder();
            sb.AppendLine("MEMENTO PROGRAMARE");
            sb.AppendLine("==================");
            sb.AppendLine($"Stimate/Stimată {appointment.ClientName},");
            sb.AppendLine();
            sb.AppendLine("Aceasta este o amintire prietenoasă că aveți o programare în 1 oră:");
            sb.AppendLine();
            sb.AppendLine($"Doctor: {appointment.DoctorName} ({appointment.DoctorSpecialization})");
            sb.AppendLine($"Data: {appointment.AppointmentDate}");
            sb.AppendLine($"Ora: {appointment.AppointmentTime}");
            sb.AppendLine($"Motivul: {appointment.Reason}");
            sb.AppendLine();
            sb.AppendLine("Vă rugăm să vă amintiți să:");
            sb.AppendLine("- Ajungeți cu 15 minute mai devreme");
            sb.AppendLine("- Aduceți actul de identitate și cardul de asigurări");
            sb.AppendLine("- Aduceți documentele medicale relevante");
            sb.AppendLine();
            sb.AppendLine("Vă mulțumim că ați ales MedanoClinic!");
            sb.AppendLine();
            sb.AppendLine("MedanoClinic - Sănătatea dumneavoastră, prioritatea noastră");
            return sb.ToString();
        }

        private string GenerateAppointmentCancelledHtml(AppointmentResponseDto appointment)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html><head><meta charset=\"UTF-8\"><title>Programare Anulată</title></head>");
            sb.AppendLine("<body style=\"font-family: Arial, sans-serif; line-height: 1.6; color: #333;\">");
            sb.AppendLine("<div style=\"max-width: 600px; margin: 0 auto; padding: 20px;\">");
            sb.AppendLine("<h2 style=\"color: #dc3545;\">Programare Anulată</h2>");
            sb.AppendLine($"<p>Stimate/Stimată {appointment.ClientName},</p>");
            sb.AppendLine("<p>Regretăm să vă informăm că programarea dumneavoastră a fost anulată:</p>");
            sb.AppendLine("<div style=\"background-color: #f8d7da; padding: 15px; border-radius: 5px; margin: 20px 0; border-left: 4px solid #dc3545;\">");
            sb.AppendLine($"<p><strong>Doctor:</strong> {appointment.DoctorName} ({appointment.DoctorSpecialization})</p>");
            sb.AppendLine($"<p><strong>Data inițială:</strong> {appointment.AppointmentDate}</p>");
            sb.AppendLine($"<p><strong>Ora inițială:</strong> {appointment.AppointmentTime}</p>");
            sb.AppendLine($"<p><strong>Motivul:</strong> {appointment.Reason}</p>");
            sb.AppendLine("</div>");
            sb.AppendLine("<p>Vă rugăm să ne contactați pentru a reprograma consultația la o oră convenabilă pentru dumneavoastră.</p>");
            sb.AppendLine("<p>Ne cerem scuze pentru orice neplăcere cauzată.</p>");
            sb.AppendLine("<p>Vă mulțumim pentru înțelegere.</p>");
            sb.AppendLine("<hr style=\"margin: 30px 0;\">");
            sb.AppendLine("<p style=\"font-size: 12px; color: #666;\">MedanoClinic - Sănătatea dumneavoastră, prioritatea noastră</p>");
            sb.AppendLine("</div></body></html>");
            return sb.ToString();
        }

        private string GenerateAppointmentCancelledPlainText(AppointmentResponseDto appointment)
        {
            var sb = new StringBuilder();
            sb.AppendLine("PROGRAMARE ANULATĂ");
            sb.AppendLine("==================");
            sb.AppendLine($"Stimate/Stimată {appointment.ClientName},");
            sb.AppendLine();
            sb.AppendLine("Regretăm să vă informăm că programarea dumneavoastră a fost anulată:");
            sb.AppendLine();
            sb.AppendLine($"Doctor: {appointment.DoctorName} ({appointment.DoctorSpecialization})");
            sb.AppendLine($"Data inițială: {appointment.AppointmentDate}");
            sb.AppendLine($"Ora inițială: {appointment.AppointmentTime}");
            sb.AppendLine($"Motivul: {appointment.Reason}");
            sb.AppendLine();
            sb.AppendLine("Vă rugăm să ne contactați pentru a reprograma consultația la o oră convenabilă pentru dumneavoastră.");
            sb.AppendLine();
            sb.AppendLine("Ne cerem scuze pentru orice neplăcere cauzată.");
            sb.AppendLine();
            sb.AppendLine("Vă mulțumim pentru înțelegere.");
            sb.AppendLine();
            sb.AppendLine("MedanoClinic - Sănătatea dumneavoastră, prioritatea noastră");
            return sb.ToString();
        }
    }
}