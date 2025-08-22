-- ===================================================================
-- COMPLETE DATABASE RESET SCRIPT (USE WITH CAUTION!)
-- This script will reset all custom tables and recreate them properly
-- WARNING: This will delete all appointment, doctor, and review data!
-- ===================================================================

PRINT 'WARNING: This script will delete all custom data!';
PRINT 'Press Ctrl+C to cancel, or continue to reset the database...';
WAITFOR DELAY '00:00:05';

-- Step 1: Drop all custom tables in correct order (respecting foreign keys)
PRINT 'Dropping existing tables...';

-- Drop Reviews table first (it has FK to Doctors and Appointments)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Reviews]') AND type in (N'U'))
BEGIN
    DROP TABLE [Reviews];
    PRINT '? Reviews table dropped';
END

-- Drop Appointments table (it has FK to Doctors)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Appointments]') AND type in (N'U'))
BEGIN
    DROP TABLE [Appointments];
    PRINT '? Appointments table dropped';
END

-- Drop Doctors table
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Doctors]') AND type in (N'U'))
BEGIN
    DROP TABLE [Doctors];
    PRINT '? Doctors table dropped';
END

-- Step 2: Remove migration history for custom migrations
DELETE FROM [__EFMigrationsHistory] 
WHERE [MigrationId] IN (
    '20250104000000_AddAppointmentsAndUserCreatedAt',
    '20250104120000_AddDoctorReviewAndUpdateAppointment',
    '20250104140000_CreateDoctorsTable'
);
PRINT '? Cleaned migration history';

-- Step 3: Recreate tables with correct structure
PRINT 'Creating tables with correct structure...';

-- Create Doctors table
CREATE TABLE [dbo].[Doctors] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [UserId] nvarchar(450) NOT NULL,
    [Specialization] nvarchar(max) NOT NULL,
    [Phone] nvarchar(max) NULL,
    [IsActive] bit NOT NULL CONSTRAINT [DF_Doctors_IsActive] DEFAULT ((1)),
    [CreatedAt] datetime2(7) NOT NULL CONSTRAINT [DF_Doctors_CreatedAt] DEFAULT (GETUTCDATE()),
    CONSTRAINT [PK_Doctors] PRIMARY KEY CLUSTERED ([Id] ASC)
);

-- Add foreign key constraint to Users
ALTER TABLE [dbo].[Doctors] WITH CHECK ADD CONSTRAINT [FK_Doctors_AspNetUsers_UserId] 
FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]);

ALTER TABLE [dbo].[Doctors] CHECK CONSTRAINT [FK_Doctors_AspNetUsers_UserId];

-- Create index on UserId
CREATE NONCLUSTERED INDEX [IX_Doctors_UserId] ON [dbo].[Doctors] ([UserId] ASC);

PRINT '? Doctors table created';

-- Create Appointments table with correct structure
CREATE TABLE [dbo].[Appointments] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [PatientId] nvarchar(450) NOT NULL,
    [DoctorId] int NOT NULL,
    [AppointmentDate] datetime2(7) NOT NULL,
    [AppointmentTime] time(7) NOT NULL CONSTRAINT [DF_Appointments_AppointmentTime] DEFAULT ('00:00:00'),
    [Reason] nvarchar(max) NOT NULL CONSTRAINT [DF_Appointments_Reason] DEFAULT (''),
    [Notes] nvarchar(max) NULL,
    [Status] int NOT NULL CONSTRAINT [DF_Appointments_Status] DEFAULT ((0)),
    [CreatedAt] datetime2(7) NOT NULL CONSTRAINT [DF_Appointments_CreatedAt] DEFAULT (GETUTCDATE()),
    [CompletedAt] datetime2(7) NULL,
    CONSTRAINT [PK_Appointments] PRIMARY KEY CLUSTERED ([Id] ASC)
);

-- Add foreign key constraints
ALTER TABLE [dbo].[Appointments] WITH CHECK ADD CONSTRAINT [FK_Appointments_AspNetUsers_PatientId] 
FOREIGN KEY([PatientId]) REFERENCES [dbo].[AspNetUsers] ([Id]);

ALTER TABLE [dbo].[Appointments] CHECK CONSTRAINT [FK_Appointments_AspNetUsers_PatientId];

ALTER TABLE [dbo].[Appointments] WITH CHECK ADD CONSTRAINT [FK_Appointments_Doctors_DoctorId] 
FOREIGN KEY([DoctorId]) REFERENCES [dbo].[Doctors] ([Id]);

ALTER TABLE [dbo].[Appointments] CHECK CONSTRAINT [FK_Appointments_Doctors_DoctorId];

-- Create indexes
CREATE NONCLUSTERED INDEX [IX_Appointments_PatientId] ON [dbo].[Appointments] ([PatientId] ASC);
CREATE NONCLUSTERED INDEX [IX_Appointments_DoctorId] ON [dbo].[Appointments] ([DoctorId] ASC);

PRINT '? Appointments table created';

-- Create Reviews table
CREATE TABLE [dbo].[Reviews] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [ClientId] nvarchar(450) NOT NULL,
    [DoctorId] int NOT NULL,
    [AppointmentId] int NOT NULL,
    [Rating] int NOT NULL,
    [Comment] nvarchar(max) NULL,
    [CreatedAt] datetime2(7) NOT NULL CONSTRAINT [DF_Reviews_CreatedAt] DEFAULT (GETUTCDATE()),
    CONSTRAINT [PK_Reviews] PRIMARY KEY CLUSTERED ([Id] ASC)
);

-- Add foreign key constraints
ALTER TABLE [dbo].[Reviews] WITH CHECK ADD CONSTRAINT [FK_Reviews_AspNetUsers_ClientId] 
FOREIGN KEY([ClientId]) REFERENCES [dbo].[AspNetUsers] ([Id]);

ALTER TABLE [dbo].[Reviews] CHECK CONSTRAINT [FK_Reviews_AspNetUsers_ClientId];

ALTER TABLE [dbo].[Reviews] WITH CHECK ADD CONSTRAINT [FK_Reviews_Doctors_DoctorId] 
FOREIGN KEY([DoctorId]) REFERENCES [dbo].[Doctors] ([Id]);

ALTER TABLE [dbo].[Reviews] CHECK CONSTRAINT [FK_Reviews_Doctors_DoctorId];

ALTER TABLE [dbo].[Reviews] WITH CHECK ADD CONSTRAINT [FK_Reviews_Appointments_AppointmentId] 
FOREIGN KEY([AppointmentId]) REFERENCES [dbo].[Appointments] ([Id]);

ALTER TABLE [dbo].[Reviews] CHECK CONSTRAINT [FK_Reviews_Appointments_AppointmentId];

-- Create indexes
CREATE NONCLUSTERED INDEX [IX_Reviews_ClientId] ON [dbo].[Reviews] ([ClientId] ASC);
CREATE NONCLUSTERED INDEX [IX_Reviews_DoctorId] ON [dbo].[Reviews] ([DoctorId] ASC);
CREATE NONCLUSTERED INDEX [IX_Reviews_AppointmentId] ON [dbo].[Reviews] ([AppointmentId] ASC);

PRINT '? Reviews table created';

-- Step 4: Update migration history
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES 
    (N'20250104000000_AddAppointmentsAndUserCreatedAt', N'6.0.0'),
    (N'20250104140000_CreateDoctorsTable', N'6.0.0');

PRINT '? Migration history updated';

-- Step 5: Verify everything is working
PRINT '';
PRINT '==================== VERIFICATION ====================';

-- Show all tables
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE' 
AND TABLE_NAME IN ('Doctors', 'Appointments', 'Reviews')
ORDER BY TABLE_NAME;

-- Verify foreign key relationships
SELECT 
    fk.name AS ForeignKeyName,
    tp.name AS ParentTable,
    cp.name AS ParentColumn,
    tr.name AS ReferencedTable,
    cr.name AS ReferencedColumn
FROM sys.foreign_keys fk
INNER JOIN sys.tables tp ON fk.parent_object_id = tp.object_id
INNER JOIN sys.tables tr ON fk.referenced_object_id = tr.object_id
INNER JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
INNER JOIN sys.columns cp ON fkc.parent_column_id = cp.column_id AND fkc.parent_object_id = cp.object_id
INNER JOIN sys.columns cr ON fkc.referenced_column_id = cr.column_id AND fkc.referenced_object_id = cr.object_id
WHERE tp.name IN ('Doctors', 'Appointments', 'Reviews')
ORDER BY tp.name, fk.name;

PRINT '';
PRINT 'Database reset completed successfully!';
PRINT 'You can now run your application and it should work properly.';
PRINT '============================================================';