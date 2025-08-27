# DoctorController - New Endpoints Documentation

## Overview
Added two new endpoints to the DoctorController to retrieve doctor-specific data: reviews and appointments.

## New Endpoints

### 1. Get Doctor Reviews
**Endpoint**: `GET /api/doctor/{doctorId}/reviews`

**Authorization**: Required (Doctor or Admin role)

**Description**: Retrieves all reviews for a specific doctor.

**Parameters**:
- `doctorId` (int, path): The ID of the doctor

**Response**: List of `ReviewDto` objects

**Example Request**:
```http
GET /api/doctor/1/reviews
Authorization: Bearer <jwt-token>
```

**Example Response**:
```json
[
  {
    "id": "1",
    "doctorId": "1",
    "clientId": "client-guid-123",
    "clientName": "John Doe",
    "rating": 5,
    "comment": "Excellent doctor, very professional and caring.",
    "createdAt": "2025-01-04T10:30:45.123Z",
    "appointmentId": "123"
  },
  {
    "id": "2",
    "doctorId": "1",
    "clientId": "client-guid-456",
    "clientName": "Jane Smith",
    "rating": 4,
    "comment": "Good experience overall.",
    "createdAt": "2025-01-03T14:20:30.456Z",
    "appointmentId": "124"
  }
]
```

### 2. Get Doctor Appointments
**Endpoint**: `GET /api/doctor/{doctorId}/appointments`

**Authorization**: Required (Doctor or Admin role)

**Description**: Retrieves all appointments for a specific doctor.

**Parameters**:
- `doctorId` (int, path): The ID of the doctor

**Response**: List of `AppointmentResponseDto` objects

**Example Request**:
```http
GET /api/doctor/1/appointments
Authorization: Bearer <jwt-token>
```

**Example Response**:
```json
[
  {
    "id": "123",
    "clientId": "client-guid-123",
    "clientName": "John Doe",
    "doctorId": "1",
    "doctorName": "Dr. Sarah Johnson",
    "doctorSpecialization": "Cardiology",
    "appointmentDate": "2025-01-05",
    "appointmentTime": "10:00",
    "status": "scheduled",
    "reason": "Routine checkup",
    "notes": "Patient has no allergies",
    "createdAt": "2025-01-04T09:15:30.789Z"
  },
  {
    "id": "124",
    "clientId": "client-guid-456",
    "clientName": "Jane Smith",
    "doctorId": "1",
    "doctorName": "Dr. Sarah Johnson",
    "doctorSpecialization": "Cardiology",
    "appointmentDate": "2025-01-04",
    "appointmentTime": "14:30",
    "status": "completed",
    "reason": "Follow-up consultation",
    "notes": "Blood pressure check",
    "createdAt": "2025-01-03T11:22:15.234Z"
  }
]
```

## Implementation Details

### Repository Layer Changes
1. **IReviewRepository**: Added `GetDoctorReviewsAsync(int doctorId)` method
2. **IAppointmentRepository**: Added `GetDoctorAppointmentsAsync(int doctorId)` method
3. **ReviewRepository**: Implemented `GetDoctorReviewsAsync` with filtering by doctorId
4. **AppointmentRepository**: Implemented `GetDoctorAppointmentsAsync` with filtering by doctorId

### Service Layer Changes
1. **IDoctorService**: Added two new method signatures
2. **DoctorService**: 
   - Added `IReviewRepository` and `IAppointmentRepository` dependencies
   - Implemented both new methods with error handling and logging

### Controller Layer Changes
1. **DoctorController**: Added two new GET endpoints with route parameters
2. Both endpoints include comprehensive error handling and logging

## Data Filtering and Ordering

### Reviews
- **Filter**: Only reviews for the specified doctor (`r.DoctorId == doctorId`)
- **Order**: Most recent reviews first (`OrderByDescending(r => r.CreatedAt)`)
- **Includes**: Client information and doctor information with user details

### Appointments
- **Filter**: Only appointments for the specified doctor (`a.DoctorId == doctorId`)
- **Order**: Most recent appointments first (`OrderByDescending(a => a.AppointmentDate)`)
- **Includes**: Patient information and doctor information with user details
- **Status Mapping**: Enum values mapped to lowercase strings for frontend compatibility

## Security Considerations

1. **Authorization**: Both endpoints require authentication with Doctor or Admin roles
2. **Parameter Validation**: Route parameters are automatically validated by ASP.NET Core
3. **Error Handling**: Sensitive information is not exposed in error responses
4. **Logging**: All operations are logged for audit purposes

## Error Responses

**500 Internal Server Error**:
```json
{
  "message": "Internal server error",
  "error": "Error message (in development only)"
}
```

**401 Unauthorized** (if not authenticated):
```json
{
  "message": "Unauthorized"
}
```

**403 Forbidden** (if wrong role):
```json
{
  "message": "Access denied"
}
```

## Use Cases

### For Doctors
- View all their appointments in one place
- See all reviews/feedback from patients
- Monitor their schedule and patient interactions

### For Admins
- Monitor doctor performance through reviews
- Oversee appointment management for specific doctors
- Generate reports on doctor activity and patient satisfaction

## Performance Considerations

- Both endpoints use Entity Framework's `Include()` for efficient data loading
- Database queries are optimized with appropriate filtering
- Results are ordered at the database level
- No N+1 query problems due to proper eager loading