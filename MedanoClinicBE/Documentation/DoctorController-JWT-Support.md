# DoctorController - Updated Endpoints for JWT Token Support

## Overview
Updated the DoctorController to properly handle doctor identification using JWT tokens (UserId/GUID) instead of just the numeric Doctor.Id. Added new endpoints that work with the logged-in doctor's identity.

## Problem Addressed
- JWT tokens contain `UserId` (GUID string from AspNetUsers)
- Previous endpoints used `Doctor.Id` (int auto-increment)
- Doctors couldn't access their own data using their JWT token identity

## New Endpoints Added

### 1. Get Current Doctor's Reviews
**Endpoint**: `GET /api/doctor/my-reviews`

**Authorization**: Doctor role only

**Description**: Retrieves reviews for the currently logged-in doctor using JWT token.

**Example Request**:
```http
GET /api/doctor/my-reviews
Authorization: Bearer <doctor-jwt-token>
```

### 2. Get Current Doctor's Appointments
**Endpoint**: `GET /api/doctor/my-appointments`

**Authorization**: Doctor role only

**Description**: Retrieves appointments for the currently logged-in doctor using JWT token.

**Example Request**:
```http
GET /api/doctor/my-appointments
Authorization: Bearer <doctor-jwt-token>
```

### 3. Get Doctor Reviews by UserId (Admin Only)
**Endpoint**: `POST /api/doctor/reviews-by-userid`

**Authorization**: Admin role only

**Description**: Allows admins to query reviews for a specific doctor using their UserId.

**Payload**: `DoctorIdRequestDto`

**Example Request**:
```http
POST /api/doctor/reviews-by-userid
Authorization: Bearer <admin-jwt-token>
Content-Type: application/json

{
  "doctorUserId": "doctor-guid-123-456-789"
}
```

### 4. Get Doctor Appointments by UserId (Admin Only)
**Endpoint**: `POST /api/doctor/appointments-by-userid`

**Authorization**: Admin role only

**Description**: Allows admins to query appointments for a specific doctor using their UserId.

**Payload**: `DoctorIdRequestDto`

**Example Request**:
```http
POST /api/doctor/appointments-by-userid
Authorization: Bearer <admin-jwt-token>
Content-Type: application/json

{
  "doctorUserId": "doctor-guid-123-456-789"
}
```

## New DTO Created

### DoctorIdRequestDto
```csharp
public class DoctorIdRequestDto
{
    [Required]
    public string DoctorUserId { get; set; }
}
```

## Implementation Details

### Repository Layer Changes
1. **IDoctorRepository**: Added `GetDoctorByUserIdAsync(string userId)` method
2. **DoctorRepository**: Implemented method to find Doctor entity by UserId with user details included

### Service Layer Changes
1. **IDoctorService**: Added two new methods:
   - `GetDoctorReviewsByUserIdAsync(string userId)`
   - `GetDoctorAppointmentsByUserIdAsync(string userId)`
2. **DoctorService**: Implemented methods that:
   - Find doctor by UserId
   - Return empty list if doctor not found (with warning log)
   - Use existing repository methods with Doctor.Id

### Controller Layer Changes
1. **Four new endpoints** with proper authorization
2. **JWT token handling** using `User.FindFirst(ClaimTypes.NameIdentifier)`
3. **Admin-specific endpoints** for querying specific doctors
4. **Doctor-specific endpoints** for self-service

## Data Flow

### For Doctors (My Data)
1. Extract `UserId` from JWT token (`ClaimTypes.NameIdentifier`)
2. Find `Doctor` record using `UserId`
3. Use `Doctor.Id` to query reviews/appointments
4. Return data or empty list if doctor not found

### For Admins (Query Specific Doctor)
1. Receive `DoctorUserId` in request payload
2. Find `Doctor` record using provided `UserId`
3. Use `Doctor.Id` to query reviews/appointments
4. Return data or empty list if doctor not found

## Database Relationships

```
AspNetUsers (Id: GUID) 
    ? (UserId)
Doctor (Id: int, UserId: string)
    ? (DoctorId)
Reviews/Appointments (DoctorId: int)
```

## Security Considerations

1. **Doctor Endpoints**: Only doctors can access their own data
2. **Admin Endpoints**: Only admins can query specific doctors by UserId
3. **JWT Validation**: Proper token extraction and validation
4. **No Data Leakage**: Returns empty list instead of errors when doctor not found
5. **Logging**: Comprehensive logging for troubleshooting without exposing sensitive data

## Backward Compatibility

**Maintained**: All existing endpoints still work:
- `GET /api/doctor/{doctorId}/reviews`
- `GET /api/doctor/{doctorId}/appointments`

**Added**: New JWT-based endpoints for better user experience

## Use Cases

### For Doctors
- **Dashboard**: Use `GET /api/doctor/my-reviews` and `GET /api/doctor/my-appointments`
- **Self-Service**: No need to know their internal Doctor.Id
- **JWT-Based**: Works seamlessly with authentication system

### For Admins
- **User Management**: Query specific doctor data using their User ID
- **Support**: Help doctors by looking up their data
- **Reporting**: Generate reports for specific doctors

## Error Handling

### Authentication Errors
```json
{
  "message": "Unable to identify user"
}
```

### Not Found (No Error - Returns Empty List)
```json
[]
```

### Server Errors
```json
{
  "message": "Internal server error",
  "error": "Error details (development only)"
}
```

## API Usage Examples

### Doctor Getting Own Data
```javascript
// Get my reviews
const myReviews = await fetch('/api/doctor/my-reviews', {
  headers: { 'Authorization': `Bearer ${doctorToken}` }
});

// Get my appointments  
const myAppointments = await fetch('/api/doctor/my-appointments', {
  headers: { 'Authorization': `Bearer ${doctorToken}` }
});
```

### Admin Querying Specific Doctor
```javascript
// Get specific doctor's reviews
const doctorReviews = await fetch('/api/doctor/reviews-by-userid', {
  method: 'POST',
  headers: { 
    'Authorization': `Bearer ${adminToken}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    doctorUserId: 'doctor-guid-123-456-789'
  })
});
```