namespace MedanoClinicBE.DTOs
{
    public class NotificationDto
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public string? AppointmentId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
        public bool EmailSent { get; set; } = false;
    }

    public class EmailNotificationDto
    {
        public string ToEmail { get; set; } = string.Empty;
        public string ToName { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string HtmlBody { get; set; } = string.Empty;
        public string? PlainTextBody { get; set; }
        public NotificationType Type { get; set; }
        public string? AppointmentId { get; set; }
    }

    public enum NotificationType
    {
        AppointmentCreated,
        AppointmentModified,
        AppointmentReminder,
        AppointmentCancelled,
        AppointmentCompleted
    }
}