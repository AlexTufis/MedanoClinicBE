# ?? Appointment Hours Management System

## Overview
The appointment hours management system allows dynamic management of doctor availability instead of hardcoded frontend hours. Each doctor can have specific appointment hours for different days, with an `IsActive` flag for flexible scheduling.

## Database Schema

### AppointmentHours Table
```sql
CREATE TABLE [AppointmentHours] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [DoctorId] int NOT NULL,
    [Hour] time NOT NULL,
    [DayOfWeek] int NOT NULL,
    [IsActive] bit NOT NULL DEFAULT 1,
    [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_AppointmentHours] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AppointmentHours_Doctors_DoctorId] FOREIGN KEY ([DoctorId]) 
        REFERENCES [Doctors] ([Id]) ON DELETE NO ACTION
);

-- Unique constraint to prevent duplicate hours for same doctor/day
CREATE UNIQUE INDEX [IX_AppointmentHours_DoctorId_Hour_DayOfWeek] 
ON [AppointmentHours] ([DoctorId], [Hour], [DayOfWeek]);
```

### Key Features
- **DoctorId**: Links to specific doctor
- **Hour**: Time slot (e.g., 09:00, 14:30)
- **DayOfWeek**: 0=Sunday, 1=Monday, ... 6=Saturday
- **IsActive**: Enables/disables specific time slots without deletion
- **Unique Constraint**: Prevents duplicate hours per doctor/day

## API Endpoints

### ?? Client Endpoints
**Base Route**: `/api/client`

#### Get All Doctors' Appointment Hours
```http
GET /api/client/appointment-hours
Authorization: Bearer {jwt-token}
```

**Response Example:**
```json
[
  {
    "doctorId": "1",
    "doctorName": "Dr. John Smith",
    "doctorSpecialization": "Cardiology",
    "appointmentHours": [
      {
        "id": "1",
        "doctorId": "1",
        "hour": "09:00",
        "dayOfWeek": "Monday",
        "isActive": true,
        "createdAt": "2025-01-04T10:30:45.123Z",
        "updatedAt": null
      }
    ]
  }
]
```

#### Get Specific Doctor's Hours
```http
GET /api/client/appointment-hours/doctor/{doctorId}
Authorization: Bearer {jwt-token}
```

#### Get Doctor's Hours for Specific Day
```http
GET /api/client/appointment-hours/doctor/{doctorId}/day/{dayOfWeek}
Authorization: Bearer {jwt-token}
```

**Example**: `GET /api/client/appointment-hours/doctor/1/day/Monday`

### ?? Admin Endpoints
**Base Route**: `/api/admin`

#### Get All Doctors' Appointment Hours
```http
GET /api/admin/appointment-hours
Authorization: Bearer {admin-jwt-token}
```

#### Create New Appointment Hour
```http
POST /api/admin/appointment-hours
Authorization: Bearer {admin-jwt-token}
Content-Type: application/json

{
  "doctorId": "1",
  "hour": "10:00",
  "dayOfWeek": "Monday",
  "isActive": true
}
```

#### Update Appointment Hour (Toggle Active Status)
```http
PUT /api/admin/appointment-hours/{id}
Authorization: Bearer {admin-jwt-token}
Content-Type: application/json

{
  "isActive": false
}
```

#### Delete Appointment Hour
```http
DELETE /api/admin/appointment-hours/{id}
Authorization: Bearer {admin-jwt-token}
```

## Default Hours Seeding

### Automatic Seeding
When the application starts, it automatically seeds default hours:

**Weekdays (Monday-Friday)**: 9:00 AM - 9:00 PM (every hour)
- 09:00, 10:00, 11:00, 12:00, 13:00, 14:00, 15:00, 16:00, 17:00, 18:00, 19:00, 20:00, 21:00

**Saturday**: 9:00 AM - 1:00 PM (reduced hours)
- 09:00, 10:00, 11:00, 12:00, 13:00

**Sunday**: No hours (day off)

### Custom Seeding
You can modify `AppointmentHourSeeder.cs` to customize default hours:

```csharp
// Example: Add Sunday hours
appointmentHours.Add(new AppointmentHour
{
    DoctorId = doctor.Id,
    DayOfWeek = DayOfWeek.Sunday,
    Hour = new TimeSpan(10, 0, 0), // 10:00 AM
    IsActive = true,
    CreatedAt = DateTime.UtcNow
});
```

## Frontend Integration

### Day of Week Mapping
```javascript
const dayMapping = {
  0: 'Sunday',
  1: 'Monday',
  2: 'Tuesday',
  3: 'Wednesday',
  4: 'Thursday',
  5: 'Friday',
  6: 'Saturday'
};
```

### Example Frontend Usage
```javascript
// Get available hours for doctor on specific day
const getDoctorHours = async (doctorId, dayOfWeek) => {
  const response = await fetch(
    `/api/client/appointment-hours/doctor/${doctorId}/day/${dayOfWeek}`,
    {
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    }
  );
  return await response.json();
};

// Filter only active hours
const activeHours = hours.filter(hour => hour.isActive);
```

## Use Cases

### 1. **Flexible Doctor Schedules**
- Different doctors can have different working hours
- Easy to accommodate part-time doctors
- Support for varying specializations with different hour requirements

### 2. **Temporary Schedule Changes**
- Doctor on vacation: Set `isActive = false` for all their hours
- Emergency coverage: Temporarily activate hours for another doctor
- Holiday schedules: Disable specific days without deleting data

### 3. **Special Appointments**
- VIP patients: Activate special hours (e.g., early morning, late evening)
- Emergency slots: Create additional hours when needed
- Seasonal adjustments: Modify hours based on demand

## Database Operations

### Run Application
The application automatically:
1. ? Creates AppointmentHours table if it doesn't exist
2. ? Sets up foreign key relationships
3. ? Creates unique indexes
4. ? Seeds default appointment hours for all active doctors

### Manual Database Update
If you need to run manually:
```bash
cd MedanoClinicBE
dotnet run
```

The database initialization is handled automatically in `Program.cs`.

## Error Handling

### Common Errors and Solutions

**Duplicate Hour Error:**
```json
{
  "message": "An appointment hour already exists for this doctor at this time on this day"
}
```
*Solution*: Check existing hours before creating new ones.

**Doctor Not Found:**
```json
{
  "message": "Doctor with the specified ID does not exist"
}
```
*Solution*: Verify the doctor exists and is active.

**Invalid Time Format:**
```json
{
  "message": "Hour must be in HH:mm format (e.g., 09:00, 14:30)"
}
```
*Solution*: Use proper time format in requests.

## Testing the System

### 1. **Run Application**
```bash
cd MedanoClinicBE
dotnet run
```

### 2. **Check Database**
```sql
SELECT * FROM AppointmentHours ORDER BY DoctorId, DayOfWeek, Hour;
```

### 3. **Test Client Endpoints**
```bash
# Get all doctors' hours
curl -H "Authorization: Bearer YOUR_TOKEN" http://localhost:5000/api/client/appointment-hours

# Get specific doctor's hours
curl -H "Authorization: Bearer YOUR_TOKEN" http://localhost:5000/api/client/appointment-hours/doctor/1
```

### 4. **Test Admin Endpoints**
```bash
# Create new appointment hour
curl -X POST http://localhost:5000/api/admin/appointment-hours \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"doctorId":"1","hour":"22:00","dayOfWeek":"Monday","isActive":true}'
```

## Migration Notes

### From Hardcoded Hours
1. **Before**: Hours were hardcoded in frontend (9 AM - 9 PM)
2. **After**: Hours are dynamically loaded from database
3. **Benefits**: 
   - Flexible scheduling per doctor
   - Easy schedule modifications
   - Better appointment management
   - Historical tracking of schedule changes

### Database Compatibility
- ? Works with existing Appointments table
- ? No changes required to existing appointment booking flow
- ? Backward compatible with current system
- ? Can be gradually adopted in frontend