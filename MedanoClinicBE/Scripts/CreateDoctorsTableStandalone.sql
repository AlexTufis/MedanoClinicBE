-- ===================================================================
-- STANDALONE SQL SCRIPT TO CREATE DOCTORS AND REVIEWS TABLES
-- Execute this script in SQL Server Management Studio if migrations fail
-- ===================================================================

-- Use your database (replace with your actual database name)
-- USE YourDatabaseName;

-- Step 1: Create Doctors table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Doctors]') AND type in (N'U'))
BEGIN
    PRINT 'Creating Doctors table...';
    
    CREATE TABLE [dbo].[Doctors] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [Specialization] nvarchar(max) NOT NULL,
        [Phone] nvarchar(max) NULL,
        [IsActive] bit NOT NULL CONSTRAINT [DF_Doctors_IsActive] DEFAULT ((1)),
        [CreatedAt] datetime2(7) NOT NULL CONSTRAINT [DF_Doctors_CreatedAt] DEFAULT (GETUTCDATE()),
        CONSTRAINT [PK_Doctors] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    -- Add foreign key constraint
    ALTER TABLE [dbo].[Doctors] WITH CHECK ADD CONSTRAINT [FK_Doctors_AspNetUsers_UserId] 
    FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]);

    ALTER TABLE [dbo].[Doctors] CHECK CONSTRAINT [FK_Doctors_AspNetUsers_UserId];

    -- Create index
    CREATE NONCLUSTERED INDEX [IX_Doctors_UserId] ON [dbo].[Doctors]
    (
        [UserId] ASC
    );

    PRINT 'Doctors table created successfully.';
END
ELSE
BEGIN
    PRINT 'Doctors table already exists.';
END

-- Step 2: Create Reviews table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Reviews]') AND type in (N'U'))
BEGIN
    PRINT 'Creating Reviews table...';
    
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

    -- Create indexes
    CREATE NONCLUSTERED INDEX [IX_Reviews_ClientId] ON [dbo].[Reviews]
    (
        [ClientId] ASC
    );

    CREATE NONCLUSTERED INDEX [IX_Reviews_DoctorId] ON [dbo].[Reviews]
    (
        [DoctorId] ASC
    );

    CREATE NONCLUSTERED INDEX [IX_Reviews_AppointmentId] ON [dbo].[Reviews]
    (
        [AppointmentId] ASC
    );

    PRINT 'Reviews table created successfully.';
END
ELSE
BEGIN
    PRINT 'Reviews table already exists.';
END

-- Step 3: Ensure Appointments table has required columns
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Appointments') AND name = 'Reason')
BEGIN
    PRINT 'Adding Reason column to Appointments table...';
    ALTER TABLE [dbo].[Appointments] ADD [Reason] nvarchar(max) NOT NULL DEFAULT ('');
    PRINT 'Reason column added successfully.';
END
ELSE
BEGIN
    PRINT 'Reason column already exists in Appointments table.';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Appointments') AND name = 'AppointmentTime')
BEGIN
    PRINT 'Adding AppointmentTime column to Appointments table...';
    ALTER TABLE [dbo].[Appointments] ADD [AppointmentTime] time(7) NOT NULL DEFAULT ('00:00:00');
    PRINT 'AppointmentTime column added successfully.';
END
ELSE
BEGIN
    PRINT 'AppointmentTime column already exists in Appointments table.';
END

-- Step 4: Update migration history (optional - only if you want EF to think the migration was applied)
-- Uncomment the following lines if you want to mark the migration as applied
/*
IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20250104140000_CreateDoctorsTable')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250104140000_CreateDoctorsTable', N'6.0.0');
    PRINT 'Migration history updated.';
END
*/

-- Step 5: Verify tables were created
PRINT '==================== VERIFICATION ====================';
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Doctors]') AND type in (N'U'))
    PRINT '? Doctors table exists';
ELSE
    PRINT '? Doctors table does NOT exist';

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Reviews]') AND type in (N'U'))
    PRINT '? Reviews table exists';
ELSE
    PRINT '? Reviews table does NOT exist';

-- Show all tables in the database
PRINT '';
PRINT 'All tables in database:';
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME;

PRINT '';
PRINT 'Script execution completed.';
PRINT '=======================================================';