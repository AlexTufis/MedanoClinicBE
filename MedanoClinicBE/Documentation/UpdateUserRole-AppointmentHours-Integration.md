# Update User Role Enhancement - AppointmentHours Integration

## Overview
Enhanced the UpdateUserRole endpoint to automatically create default AppointmentHours when assigning the Doctor role to users. This ensures that newly created doctors have a complete setup with both their doctor record and available appointment hours.

## Changes Made

### 1. Enhanced IAppointmentHourRepository Interface
**File**: `MedanoClinicBE\Repositories\Interfaces\IAppointmentHourRepository.cs`

Added new method for creating default appointment hours:
```csharp
Task CreateDefaultAppointmentHoursForDoctorAsync(int doctorId);
```

### 2. Implemented Default Hours Creation
**File**: `MedanoClinicBE\Repositories\AppointmentHourRepository.cs`

Added implementation that creates standard appointment hours for a doctor:

```csharp
public async Task CreateDefaultAppointmentHoursForDoctorAsync(int doctorId)
{
    // Check if the doctor already has appointment hours
    var existingHours = await _context.AppointmentHours
        .Where(ah => ah.DoctorId == doctorId)
        .AnyAsync();

    if (existingHours)
        return; // Doctor already has appointment hours

    var appointmentHours = new List<AppointmentHour>();

    // Monday to Friday (9 AM to 9 PM - every hour)
    for (int day = 1; day <= 5; day++) // 1=Monday, 5=Friday
    {
        for (int hour = 9; hour <= 21; hour++)
        {
            appointmentHours.Add(new AppointmentHour
            {
                DoctorId = doctorId,
                DayOfWeek = (DayOfWeek)day,
                Hour = new TimeSpan(hour, 0, 0),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }
    }

    // Saturday - reduced hours (9 AM to 1 PM)
    for (int hour = 9; hour <= 13; hour++)
    {
        appointmentHours.Add(new AppointmentHour
        {
            DoctorId = doctorId,
            DayOfWeek = DayOfWeek.Saturday,
            Hour = new TimeSpan(hour, 0, 0),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
    }

    // No hours for Sunday (day off)

    if (appointmentHours.Any())
    {
        _context.AppointmentHours.AddRange(appointmentHours);
        await _context.SaveChangesAsync();
    }
}
```

### 3. Enhanced DoctorService
**File**: `MedanoClinicBE\Services\DoctorService.cs`

**Added Dependencies**:
- `IAppointmentHourRepository` for appointment hours management

**Enhanced Doctor Creation Logic**:
```csharp
// Create new doctor record
var newDoctor = await _doctorRepository.CreateDoctorAsync(dto.UserId, dto.Specialization!, dto.Phone);

// Create default appointment hours for the new doctor
try
{
    await _appointmentHourRepository.CreateDefaultAppointmentHoursForDoctorAsync(newDoctor.Id);
    _logger.LogInformation("Default appointment hours created for doctor {DoctorId}", newDoctor.Id);
}
catch (Exception ahEx)
{
    _logger.LogError(ahEx, "Failed to create default appointment hours for doctor {DoctorId}", newDoctor.Id);
    // Continue - doctor creation succeeded, appointment hours can be created later
}
```

### 4. Updated Controller Response
**File**: `MedanoClinicBE\Controllers\DoctorController.cs`

Enhanced success message to reflect complete doctor setup:
```csharp
var message = dto.RoleName.Equals("Doctor", StringComparison.OrdinalIgnoreCase) 
    ? "User role updated successfully, doctor record created, and default appointment hours set." 
    : "User role updated successfully";
```

## Default Appointment Hours Schedule

When a new doctor is created, the system automatically creates the following appointment hours:

### Weekdays (Monday - Friday)
- **Hours**: 9:00 AM to 9:00 PM (13 hours daily)
- **Time Slots**: 09:00, 10:00, 11:00, 12:00, 13:00, 14:00, 15:00, 16:00, 17:00, 18:00, 19:00, 20:00, 21:00
- **Total Slots**: 65 weekly slots

### Saturday
- **Hours**: 9:00 AM to 1:00 PM (5 hours)
- **Time Slots**: 09:00, 10:00, 11:00, 12:00, 13:00
- **Total Slots**: 5 weekly slots

### Sunday
- **Hours**: No hours (day off)
- **Total Slots**: 0

**Total Default Slots Per Week**: 70 appointment slots

## Database Impact

### AppointmentHours Table Updates
For each new doctor created via role assignment, the system inserts 70 records:
- 5 days × 13 hours = 65 records (Monday-Friday)
- 1 day × 5 hours = 5 records (Saturday)
- 0 records for Sunday

### Data Structure Example
```sql
-- Example records for Doctor ID 1
INSERT INTO AppointmentHours (DoctorId, DayOfWeek, Hour, IsActive, CreatedAt) VALUES
(1, 1, '09:00:00', 1, GETUTCDATE()), -- Monday 9 AM
(1, 1, '10:00:00', 1, GETUTCDATE()), -- Monday 10 AM
...
(1, 6, '13:00:00', 1, GETUTCDATE()); -- Saturday 1 PM
```

## Business Logic Flow

### Complete Doctor Role Assignment Process
1. **Validate Input** ? User exists, Role exists, Specialization provided
2. **Update Role** ? Remove old roles, assign Doctor role
3. **Create Doctor Record** ? Insert into Doctors table
4. **Create Appointment Hours** ? Insert 70 default appointment slots
5. **Success Response** ? Confirm complete setup

### Error Handling Strategy
- **Doctor Creation Fails**: Role assignment continues (can retry doctor creation)
- **Appointment Hours Fail**: Doctor record exists (can create hours later)
- **Both Fail**: Role assignment succeeds (manual cleanup may be needed)

## API Response Updates

### Success Response (Doctor Role)
```json
{
  "message": "User role updated successfully, doctor record created, and default appointment hours set."
}
```

### Success Response (Other Roles)
```json
{
  "message": "User role updated successfully"
}
```

### Error Handling
The system provides detailed logging but continues processing even if appointment hours creation fails, ensuring core functionality (role assignment and doctor record) succeeds.

## Benefits Achieved

### 1. **Complete Doctor Setup**
- New doctors are immediately ready to receive appointments
- No manual intervention required for basic scheduling setup
- Consistent availability across all new doctors

### 2. **Improved User Experience**
- Admins don't need separate steps to set up doctor schedules
- Clients can immediately book appointments with new doctors
- Doctors have immediate access to their schedule management

### 3. **System Reliability**
- Graceful error handling ensures core operations succeed
- Detailed logging for troubleshooting appointment hour issues
- Duplicate prevention ensures safe re-execution

### 4. **Operational Efficiency**
- Reduces manual setup tasks
- Standardizes doctor availability patterns
- Simplifies onboarding process

## Use Cases

### Admin Creating New Doctor
1. Admin assigns Doctor role with specialization
2. System creates doctor record automatically
3. System sets up 70 default appointment hours
4. Doctor can immediately manage their schedule
5. Clients can book appointments right away

### Converting Client to Doctor
1. Existing client user gets Doctor role
2. Previous client appointments remain intact
3. New doctor capabilities are added
4. Complete scheduling setup is automatic

## Testing Scenarios

### Successful Doctor Creation
- Role assignment + Doctor record + Appointment hours ?
- All 70 appointment hours created ?
- Success message includes appointment hours confirmation ?

### Partial Failures
- Role assignment succeeds, doctor creation fails ? ? Role assigned, manual doctor creation needed
- Doctor creation succeeds, appointment hours fail ? ? Doctor exists, manual hours setup needed
- Both succeed ? ? Complete setup

### Duplicate Prevention
- Existing doctor record ? No duplicate created ?
- Existing appointment hours ? No duplicates created ?

## Monitoring & Maintenance

### Log Messages to Monitor
- `"Doctor record created for user {UserId} with specialization {Specialization}"`
- `"Default appointment hours created for doctor {DoctorId}"`
- `"Failed to create default appointment hours for doctor {DoctorId}"`

### Database Queries for Verification
```sql
-- Check doctor appointment hours after creation
SELECT COUNT(*) as HourCount 
FROM AppointmentHours 
WHERE DoctorId = @DoctorId;
-- Expected: 70 hours for new doctor

-- Verify hour distribution
SELECT DayOfWeek, COUNT(*) as HourCount
FROM AppointmentHours 
WHERE DoctorId = @DoctorId
GROUP BY DayOfWeek
ORDER BY DayOfWeek;
-- Expected: Mon-Fri: 13 each, Sat: 5, Sun: 0
```