-- ===================================================================
-- SAMPLE DATA FOR DOCTORS TABLE
-- Execute this after creating the Doctors table to add sample data
-- ===================================================================

-- First, let's create some sample users with Doctor role if they don't exist
-- Note: You'll need to replace the password hashes with actual hashed passwords

DECLARE @DoctorRoleId nvarchar(450);
SELECT @DoctorRoleId = Id FROM AspNetRoles WHERE Name = 'Doctor';

-- If Doctor role doesn't exist, create it
IF @DoctorRoleId IS NULL
BEGIN
    SET @DoctorRoleId = NEWID();
    INSERT INTO AspNetRoles (Id, Name, NormalizedName)
    VALUES (@DoctorRoleId, 'Doctor', 'DOCTOR');
    PRINT 'Doctor role created.';
END

-- Sample doctor users (you may need to adjust the password hashes)
DECLARE @User1Id nvarchar(450) = NEWID();
DECLARE @User2Id nvarchar(450) = NEWID();
DECLARE @User3Id nvarchar(450) = NEWID();

-- Insert sample doctor users if they don't exist
IF NOT EXISTS (SELECT * FROM AspNetUsers WHERE Email = 'dr.smith@clinic.com')
BEGIN
    INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, FirstName, LastName, DisplayName, CreatedAt, EmailConfirmed, LockoutEnabled)
    VALUES (@User1Id, 'dr.smith@clinic.com', 'DR.SMITH@CLINIC.COM', 'dr.smith@clinic.com', 'DR.SMITH@CLINIC.COM', 'John', 'Smith', 'Dr. John Smith', GETUTCDATE(), 1, 0);
    
    -- Assign Doctor role
    INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@User1Id, @DoctorRoleId);
END
ELSE
BEGIN
    SELECT @User1Id = Id FROM AspNetUsers WHERE Email = 'dr.smith@clinic.com';
END

IF NOT EXISTS (SELECT * FROM AspNetUsers WHERE Email = 'dr.johnson@clinic.com')
BEGIN
    INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, FirstName, LastName, DisplayName, CreatedAt, EmailConfirmed, LockoutEnabled)
    VALUES (@User2Id, 'dr.johnson@clinic.com', 'DR.JOHNSON@CLINIC.COM', 'dr.johnson@clinic.com', 'DR.JOHNSON@CLINIC.COM', 'Sarah', 'Johnson', 'Dr. Sarah Johnson', GETUTCDATE(), 1, 0);
    
    -- Assign Doctor role
    INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@User2Id, @DoctorRoleId);
END
ELSE
BEGIN
    SELECT @User2Id = Id FROM AspNetUsers WHERE Email = 'dr.johnson@clinic.com';
END

IF NOT EXISTS (SELECT * FROM AspNetUsers WHERE Email = 'dr.williams@clinic.com')
BEGIN
    INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, FirstName, LastName, DisplayName, CreatedAt, EmailConfirmed, LockoutEnabled)
    VALUES (@User3Id, 'dr.williams@clinic.com', 'DR.WILLIAMS@CLINIC.COM', 'dr.williams@clinic.com', 'DR.WILLIAMS@CLINIC.COM', 'Michael', 'Williams', 'Dr. Michael Williams', GETUTCDATE(), 1, 0);
    
    -- Assign Doctor role
    INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@User3Id, @DoctorRoleId);
END
ELSE
BEGIN
    SELECT @User3Id = Id FROM AspNetUsers WHERE Email = 'dr.williams@clinic.com';
END

-- Insert sample doctors if they don't exist
IF NOT EXISTS (SELECT * FROM Doctors WHERE UserId = @User1Id)
BEGIN
    INSERT INTO Doctors (UserId, Specialization, Phone, IsActive, CreatedAt)
    VALUES (@User1Id, 'Cardiology', '+1-555-0101', 1, GETUTCDATE());
    PRINT 'Dr. John Smith (Cardiology) added to Doctors table.';
END

IF NOT EXISTS (SELECT * FROM Doctors WHERE UserId = @User2Id)
BEGIN
    INSERT INTO Doctors (UserId, Specialization, Phone, IsActive, CreatedAt)
    VALUES (@User2Id, 'Dermatology', '+1-555-0102', 1, GETUTCDATE());
    PRINT 'Dr. Sarah Johnson (Dermatology) added to Doctors table.';
END

IF NOT EXISTS (SELECT * FROM Doctors WHERE UserId = @User3Id)
BEGIN
    INSERT INTO Doctors (UserId, Specialization, Phone, IsActive, CreatedAt)
    VALUES (@User3Id, 'Orthopedics', '+1-555-0103', 1, GETUTCDATE());
    PRINT 'Dr. Michael Williams (Orthopedics) added to Doctors table.';
END

-- Verify the data
PRINT '';
PRINT '==================== VERIFICATION ====================';
PRINT 'Sample doctors added:';

SELECT 
    d.Id,
    u.FirstName + ' ' + u.LastName as DoctorName,
    d.Specialization,
    u.Email,
    d.Phone,
    d.IsActive,
    d.CreatedAt
FROM Doctors d
INNER JOIN AspNetUsers u ON d.UserId = u.Id
ORDER BY d.Id;

PRINT '';
PRINT 'Total doctors in system: ' + CAST((SELECT COUNT(*) FROM Doctors) AS VARCHAR(10));
PRINT '=======================================================';

EXEC sp_MSforeachtable 'ALTER TABLE ? ENABLE TRIGGER ALL';
EXEC sp_MSforeachtable 'ALTER TABLE ? CHECK CONSTRAINT ALL';