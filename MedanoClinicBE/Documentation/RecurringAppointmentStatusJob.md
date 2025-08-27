# ?? Recurring Appointment Status Update Job

## Overview
A Hangfire-powered recurring job that automatically updates appointment statuses from "Scheduled" to "Completed" when their appointment date and time have passed.

## Implementation Details

### ? **Job Schedule**
- **Frequency**: Every hour (using `Cron.Hourly`)
- **Job Name**: `update-past-appointments-status`
- **Queue**: `maintenance`
- **Time Zone**: Local server time

### ?? **Job Logic**
```csharp
public async Task<int> UpdatePastAppointmentsStatusAsync()
{
    var currentDate = DateTime.Today;
    var currentTime = DateTime.Now.TimeOfDay;

    // Find scheduled appointments that are past their time
    var pastAppointments = await _context.Appointments
        .Where(a => a.Status == AppointmentStatus.Scheduled && 
                   (a.AppointmentDate < currentDate || 
                    (a.AppointmentDate == currentDate && a.AppointmentTime < currentTime)))
        .ToListAsync();

    // Update to Completed status
    foreach (var appointment in pastAppointments)
    {
        appointment.Status = AppointmentStatus.Completed; // Status = 2
        appointment.CompletedAt = DateTime.UtcNow;
    }

    await _context.SaveChangesAsync();
    return pastAppointments.Count;
}
```

### ?? **Update Criteria**
An appointment is updated to "Completed" if:
1. ? Current status is `Scheduled` (0)
2. ? Appointment date is before today, OR
3. ? Appointment date is today AND appointment time has passed

### ?? **Configuration**

#### **Hangfire Server Setup**
```csharp
builder.Services.AddHangfireServer(options =>
{
    options.SchedulePollingInterval = TimeSpan.FromSeconds(30);
    options.Queues = new[] { "maintenance", "notifications", "default" };
});
```

#### **Recurring Job Registration**
```csharp
RecurringJob.AddOrUpdate<IJobService>(
    "update-past-appointments-status",
    x => x.ProcessPastAppointmentsStatusUpdateAsync(),
    Cron.Hourly, // Runs every hour at minute 0
    TimeZoneInfo.Local);
```

## Database Impact

### **Appointments Table Updates**
```sql
-- Example of what gets updated
UPDATE Appointments 
SET Status = 2,           -- Completed
    CompletedAt = GETUTCDATE()
WHERE Status = 0          -- Scheduled
  AND (AppointmentDate < CAST(GETDATE() AS DATE) 
       OR (AppointmentDate = CAST(GETDATE() AS DATE) 
           AND AppointmentTime < CAST(GETDATE() AS TIME)));
```

### **Status Enum Values**
- `Scheduled = 0`
- `InProgress = 1`
- `Completed = 2` ? Target status
- `Cancelled = 3`
- `NoShow = 4`

## Monitoring & Logging

### **Application Logs**
```
[12:00:01] Processing past appointments status update job
[12:00:02] Updated 5 past appointments from scheduled to completed status
```

### **Hangfire Dashboard**
- **URL**: `http://localhost:5000/hangfire`
- **Recurring Jobs Tab**: View job schedule and execution history
- **Jobs Tab**: See individual job executions

### **Job Execution Data**
```json
{
  "jobName": "update-past-appointments-status",
  "schedule": "0 * * * *",
  "nextExecution": "2025-01-04T13:00:00",
  "lastExecution": "2025-01-04T12:00:00",
  "status": "Succeeded"
}
```

## Use Cases

### ?? **Automatic Status Management**
- **Past Appointments**: Automatically mark as completed
- **Clean Status**: Prevents appointments staying "scheduled" forever
- **Historical Accuracy**: Maintains correct appointment history

### ?? **Reporting Benefits**
- **Accurate Statistics**: Completed appointments count correctly
- **Dashboard Metrics**: Admin dashboard shows real completion rates
- **Billing Integration**: Completed appointments can trigger billing

### ?? **Operational Efficiency**
- **No Manual Work**: Staff doesn't need to manually update past appointments
- **Consistency**: All past appointments handled uniformly
- **Scalability**: Works regardless of appointment volume

## Testing the Job

### **1. Create Test Data**
```sql
-- Insert a past appointment that should be updated
INSERT INTO Appointments (PatientId, DoctorId, AppointmentDate, AppointmentTime, Reason, Status, CreatedAt)
VALUES ('user-id', 1, '2025-01-03', '14:00', 'Test appointment', 0, GETUTCDATE());
```

### **2. Trigger Job Manually**
```csharp
// Via Hangfire Dashboard or code
BackgroundJob.Enqueue<IJobService>(x => x.ProcessPastAppointmentsStatusUpdateAsync());
```

### **3. Verify Results**
```sql
-- Check that past appointments were updated
SELECT Id, AppointmentDate, AppointmentTime, Status, CompletedAt
FROM Appointments 
WHERE Status = 2 AND CompletedAt IS NOT NULL
ORDER BY CompletedAt DESC;
```

## Error Handling

### **Database Errors**
- Automatic retry via Hangfire
- Logged with full error details
- Job marked as failed if retries exhausted

### **No Appointments to Update**
```
[12:00:01] Processing past appointments status update job
[12:00:01] No past appointments found to update
```

### **Concurrency Protection**
- Uses Entity Framework change tracking
- Optimistic concurrency prevents conflicts
- Transaction rollback on errors

## Performance Considerations

### **Optimized Query**
```csharp
// Efficient WHERE clause with indexes
.Where(a => a.Status == AppointmentStatus.Scheduled && 
           (a.AppointmentDate < currentDate || 
            (a.AppointmentDate == currentDate && a.AppointmentTime < currentTime)))
```

### **Batch Updates**
- Loads all matching appointments into memory
- Single `SaveChangesAsync()` call
- Minimizes database round trips

### **Index Recommendations**
```sql
-- Recommended indexes for performance
CREATE INDEX IX_Appointments_Status_Date_Time 
ON Appointments (Status, AppointmentDate, AppointmentTime);
```

## Configuration Options

### **Change Schedule Frequency**
```csharp
// Every 30 minutes
Cron.Custom("*/30 * * * *")

// Every 2 hours  
Cron.Custom("0 */2 * * *")

// Daily at 2 AM
Cron.Daily(2)
```

### **Custom Time Zones**
```csharp
RecurringJob.AddOrUpdate<IJobService>(
    "update-past-appointments-status",
    x => x.ProcessPastAppointmentsStatusUpdateAsync(),
    Cron.Hourly,
    TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
```

## Production Recommendations

### ? **Monitoring**
- Set up alerts for job failures
- Monitor job execution time
- Track number of appointments updated

### ? **Backup Strategy**
- Consider status change notifications
- Maintain audit trail of status changes
- Backup before major updates

### ? **Testing**
- Test with various appointment scenarios
- Verify time zone handling
- Test job failure recovery

## Summary

The recurring job system provides automated, reliable appointment status management that:

- ?? **Runs every hour** to catch past appointments quickly
- ?? **Updates only scheduled appointments** that are past their time
- ?? **Maintains accurate statistics** for dashboards and reports  
- ?? **Requires zero manual intervention** from staff
- ?? **Scales automatically** with appointment volume

This ensures your appointment system always maintains accurate, up-to-date status information! ??