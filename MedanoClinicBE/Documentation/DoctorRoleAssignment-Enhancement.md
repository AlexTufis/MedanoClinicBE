# Doctor Role Assignment Enhancement

## Overview
Enhanced the user role update functionality to automatically create Doctor records when assigning the Doctor role to users. This ensures proper data integrity and enables doctor-specific features.

## Changes Made

### 1. Updated UpdateUserRoleDto
**File**: `MedanoClinicBE\DTOs\UpdateUserRoleDto.cs`

Added new properties for doctor creation:
```csharp
public class UpdateUserRoleDto
{
    [Required]
    public string UserId { get; set; }
    
    [Required]
    public string RoleName { get; set; }
    
    // Required when changing role to Doctor
    public string? Specialization { get; set; }
    
    // Optional when changing role to Doctor
    public string? Phone { get; set; }
}
```

### 2. Enhanced IDoctorRepository Interface
**File**: `MedanoClinicBE\Repositories\Interfaces\IDoctorRepository.cs`

Added method for creating doctor records:
```csharp
Task<Doctor> CreateDoctorAsync(string userId, string specialization, string? phone = null);
```

### 3. Implemented Doctor Creation in Repository
**File**: `MedanoClinicBE\Repositories\DoctorRepository.cs`

```csharp
public async Task<Doctor> CreateDoctorAsync(string userId, string specialization, string? phone = null)
{
    var doctor = new Doctor
    {
        UserId = userId,
        Specialization = specialization,
        Phone = phone,
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };

    _context.Doctors.Add(doctor);
    await _context.SaveChangesAsync();

    // Return the created doctor with user information
    return await _context.Doctors
        .Include(d => d.User)
        .FirstAsync(d => d.Id == doctor.Id);
}
```

### 4. Enhanced Service Layer Logic
**File**: `MedanoClinicBE\Services\DoctorService.cs`

**Key Features Added**:
- **Validation**: Ensures Specialization is provided when assigning Doctor role
- **Duplicate Prevention**: Checks if doctor record already exists before creating
- **Error Handling**: Comprehensive logging and error handling
- **Transaction Safety**: Role assignment succeeds even if doctor creation fails (with logging)

**Logic Flow**:
1. Validate user exists
2. Validate role exists
3. **NEW**: Validate Doctor role requirements (Specialization required)
4. Remove existing roles
5. Assign new role
6. **NEW**: Create doctor record if role is "Doctor"

### 5. Updated Controller Validation
**File**: `MedanoClinicBE\Controllers\DoctorController.cs`

**Enhanced Features**:
- **Input Validation**: Validates Specialization when assigning Doctor role
- **Better Error Messages**: Specific messages for Doctor role requirements
- **Success Messages**: Different messages based on role type

## API Usage

### Request Example (Doctor Role Assignment)
```http
PUT /api/doctor/update-user-role
Authorization: Bearer <admin-jwt-token>
Content-Type: application/json

{
  "userId": "user-guid-123-456-789",
  "roleName": "Doctor",
  "specialization": "Cardiology",
  "phone": "+1234567890"
}
```

### Request Example (Other Role Assignment)
```http
PUT /api/doctor/update-user-role
Authorization: Bearer <admin-jwt-token>
Content-Type: application/json

{
  "userId": "user-guid-123-456-789",
  "roleName": "Client"
}
```

### Response Examples

**Success (Doctor Role)**:
```json
{
  "message": "User role updated successfully and doctor record created."
}
```

**Success (Other Role)**:
```json
{
  "message": "User role updated successfully"
}
```

**Validation Error**:
```json
{
  "message": "Specialization is required when assigning Doctor role."
}
```

**General Error**:
```json
{
  "message": "Failed to update user role. User or role may not exist, or specialization may be missing for Doctor role."
}
```

## Database Impact

### Doctors Table Structure
```sql
CREATE TABLE [Doctors] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [UserId] nvarchar(450) NOT NULL,
    [Specialization] nvarchar(max) NOT NULL,
    [Phone] nvarchar(max) NULL,
    [IsActive] bit NOT NULL DEFAULT 1,
    [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_Doctors] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Doctors_AspNetUsers_UserId] FOREIGN KEY ([UserId]) 
        REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
);
```

### Data Relationships
```
AspNetUsers (Id: GUID)
    ? (UserId)
Doctor (Id: int, UserId: string)
    ? (DoctorId)
Appointments/Reviews (DoctorId: int)
```

## Validation Rules

### For Doctor Role Assignment
- **UserId**: Required (existing user)
- **RoleName**: Required (must be "Doctor")
- **Specialization**: Required (cannot be null/empty)
- **Phone**: Optional

### For Other Role Assignments
- **UserId**: Required (existing user)
- **RoleName**: Required (valid role name)
- **Specialization**: Ignored
- **Phone**: Ignored

## Error Handling

### Service Layer
- **User Not Found**: Returns false with warning log
- **Role Not Found**: Returns false with warning log
- **Missing Specialization**: Returns false with warning log
- **Role Assignment Failure**: Returns false with error log
- **Doctor Creation Failure**: Continues with role assignment (logs error)

### Controller Layer
- **Model Validation**: Returns 400 with validation errors
- **Missing Specialization**: Returns 400 with specific message
- **Service Failure**: Returns 400 with descriptive message
- **Exception**: Returns 500 with error details

## Security Considerations

1. **Authorization**: Only Admin users can update roles
2. **Input Validation**: Comprehensive validation at controller and service levels
3. **Data Integrity**: Ensures doctor records exist for users with Doctor role
4. **Audit Trail**: Complete logging of all operations
5. **Transaction Safety**: Role assignment prioritized over doctor creation

## Use Cases

### Admin Promoting Client to Doctor
1. Admin provides user ID, "Doctor" role, specialization, and phone
2. System validates requirements
3. System updates role in AspNetUserRoles
4. System creates doctor record in Doctors table
5. Doctor can now use doctor-specific features

### Admin Changing Doctor's Role
1. Admin provides user ID and new role
2. System updates role (doctor record remains for data integrity)
3. User loses doctor-specific access but retains data

## Testing Scenarios

### Valid Doctor Assignment
- Existing Client ? Doctor with valid specialization ?
- New user ? Doctor with valid specialization ?

### Invalid Doctor Assignment
- Missing specialization ? Error ?
- Empty specialization ? Error ?
- Non-existent user ? Error ?

### Duplicate Doctor Creation
- Existing doctor record ? No duplicate created, logs info ?

### Role Changes
- Doctor ? Client: Role updated, doctor record preserved ?
- Client ? Admin: Role updated, no doctor record needed ?