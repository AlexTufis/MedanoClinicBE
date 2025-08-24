# ?? MedanoClinic Notification System Documentation

## Overview
This notification system provides comprehensive appointment notifications including:
- ? In-app notifications for frontend display
- ? Email notifications for appointment events
- ? Scheduled reminder notifications (1 hour before appointments)
- ? Support for appointment creation, modification, cancellation, and reminder events

## Architecture

### Components
1. **EmailService** - Handles SMTP email sending with professional HTML templates
2. **NotificationService** - Manages in-app notifications and coordinates email sending
3. **JobService** - Schedules and manages appointment reminders using Timer (simple implementation)
4. **NotificationsController** - API endpoints for frontend to retrieve notifications

### Notification Types
```csharp
public enum NotificationType
{
    AppointmentCreated,     // When appointment is booked
    AppointmentModified,    // When appointment details change
    AppointmentReminder,    // 1 hour before appointment
    AppointmentCancelled,   // When appointment is cancelled
    AppointmentCompleted    // When appointment is completed
}
```

## API Endpoints

### Get User Notifications
```http
GET /api/notifications
Authorization: Bearer {jwt-token}
```

**Response Example:**
```json
[
  {
    "id": "guid-1234",
    "userId": "user-456",
    "title": "Appointment Confirmed",
    "message": "Your appointment with Dr. Sarah Johnson on 2025-01-15 at 14:30 has been confirmed.",
    "type": "AppointmentCreated",
    "appointmentId": "123",
    "createdAt": "2025-01-04T10:30:45.123Z",
    "isRead": false,
    "emailSent": true
  }
]
```

### Mark Notification as Read
```http
PUT /api/notifications/{id}/read
Authorization: Bearer {jwt-token}
```

## Configuration

### appsettings.json Email Settings
```json
{
  "EmailSettings": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-app-password",
    "EnableSsl": true,
    "FromEmail": "noreply@medanoclinic.com",
    "FromName": "MedanoClinic"
  }
}
```

### Gmail Setup Instructions
1. Enable 2-factor authentication on your Gmail account
2. Generate an App Password for your application
3. Use the App Password in SmtpPassword field
4. Update SmtpUsername with your Gmail address

### Alternative SMTP Providers
- **SendGrid**: `smtp.sendgrid.net:587`
- **Mailgun**: `smtp.mailgun.org:587`
- **Amazon SES**: `email-smtp.region.amazonaws.com:587`
- **Outlook**: `smtp-mail.outlook.com:587`

## Integration Examples

### Frontend Integration (React/TypeScript)
```typescript
// Get notifications
const fetchNotifications = async () => {
  const response = await fetch('/api/notifications', {
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    }
  });
  const notifications = await response.json();
  setNotifications(notifications);
};

// Mark as read
const markAsRead = async (notificationId: string) => {
  await fetch(`/api/notifications/${notificationId}/read`, {
    method: 'PUT',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    }
  });
};

// Real-time notifications with SignalR (optional enhancement)
const connection = new HubConnectionBuilder()
  .withUrl('/notificationHub')
  .build();

connection.on('ReceiveNotification', (notification) => {
  // Display notification in UI
  showToast(notification.title, notification.message);
});
```

## Email Templates

The system includes professional HTML email templates for:
- **Appointment Confirmation** - Welcome email with appointment details
- **Appointment Updated** - Change notification with updated details
- **Appointment Reminder** - 1-hour reminder with preparation checklist
- **Appointment Cancelled** - Cancellation notice with rescheduling information

## Scheduled Jobs

### Current Implementation (Timer-based)
- Simple Timer-based scheduling for demonstration
- Schedules reminder 1 hour before appointment
- Automatically cancels/reschedules when appointments change

### Production Recommendations

#### Option 1: Hangfire (Recommended)
```csharp
// Install: dotnet add package Hangfire
// In Program.cs:
builder.Services.AddHangfire(x => x.UseSqlServerStorage(connectionString));
builder.Services.AddHangfireServer();

// Usage:
BackgroundJob.Schedule(() => SendReminder(appointmentId), 
    appointmentDateTime.AddHours(-1));
```

#### Option 2: Quartz.NET
```csharp
// Install: dotnet add package Quartz
// More complex but very powerful scheduling
services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjection();
    q.UseSimpleTypeLoader();
    q.UseInMemoryStore();
});
```

## Production Enhancements

### 1. Database Storage
Replace in-memory notification storage with Entity Framework:
```csharp
public class Notification
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public NotificationType Type { get; set; }
    public string? AppointmentId { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
    public bool EmailSent { get; set; }
}
```

### 2. Email Queue
Implement background email queue for better reliability:
```csharp
public interface IEmailQueue
{
    Task QueueEmailAsync(EmailNotificationDto email);
    Task ProcessQueueAsync();
}
```

### 3. Notification Preferences
Allow users to configure notification preferences:
```csharp
public class NotificationPreferences
{
    public bool EmailNotifications { get; set; } = true;
    public bool AppointmentReminders { get; set; } = true;
    public int ReminderHours { get; set; } = 1; // Hours before appointment
}
```

### 4. Real-time Notifications
Add SignalR for real-time in-app notifications:
```csharp
// Install: dotnet add package Microsoft.AspNetCore.SignalR
public class NotificationHub : Hub
{
    public async Task JoinGroup(string userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, userId);
    }
}
```

### 5. Template Customization
Make email templates customizable:
```csharp
public interface ITemplateService
{
    Task<string> RenderTemplateAsync(string templateName, object model);
}
```

## Testing

### Unit Tests
```csharp
[Test]
public async Task SendAppointmentCreatedNotification_ShouldSendEmailAndCreateNotification()
{
    // Arrange
    var appointment = new AppointmentResponseDto { /* test data */ };
    
    // Act
    var result = await _notificationService.SendAppointmentCreatedNotificationAsync(appointment);
    
    // Assert
    Assert.IsTrue(result);
    // Verify email was sent
    // Verify notification was created
}
```

### Integration Tests
```csharp
[Test]
public async Task CreateAppointment_ShouldTriggerNotifications()
{
    // Test that creating an appointment triggers both
    // in-app notification and email notification
}
```

## Monitoring and Logging

The system includes comprehensive logging:
- Email send success/failure
- Notification creation/delivery
- Scheduled job execution
- Error handling and retry logic

Monitor these logs for:
- Email delivery rates
- Notification performance
- Scheduler reliability
- User engagement metrics

## Troubleshooting

### Common Issues

1. **Emails not sending**
   - Check SMTP credentials
   - Verify firewall/antivirus settings
   - Check email provider limits

2. **Reminders not scheduling**
   - Verify appointment datetime parsing
   - Check Timer disposal in JobService
   - Monitor application restarts

3. **Notifications not displaying**
   - Check user authentication
   - Verify API endpoint responses
   - Check frontend notification handling

## Future Enhancements

1. **SMS Notifications** - Add Twilio integration
2. **Push Notifications** - Mobile app notifications
3. **Notification Analytics** - Track open rates, click-through rates
4. **A/B Testing** - Test different email templates
5. **Localization** - Multi-language support
6. **Advanced Scheduling** - Multiple reminder times
7. **Notification Categories** - Allow users to subscribe to specific types