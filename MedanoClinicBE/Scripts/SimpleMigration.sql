-- ===================================================================
-- SIMPLE MIGRATION SCRIPT TO CREATE DOCTORS AND REVIEWS TABLES
-- This script avoids complex logic and focuses on creating what we need
-- ===================================================================

PRINT 'Starting simplified migration process...';

-- Step 1: Create Doctors table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Doctors]') AND type in (N'U'))
BEGIN
    PRINT 'Creating Doctors table...';
    
    CREATE TABLE [dbo].[Doctors] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [Specialization] nvarchar(max) NOT NULL,
        [Phone] nvarchar(max) NULL,
        [IsActive] bit NOT NULL DEFAULT 1,
        [CreatedAt] datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_Doctors] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    -- Add foreign key constraint to Users
    ALTER TABLE [dbo].[Doctors] WITH CHECK ADD CONSTRAINT [FK_Doctors_AspNetUsers_UserId] 
    FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]);

    ALTER TABLE [dbo].[Doctors] CHECK CONSTRAINT [FK_Doctors_AspNetUsers_UserId];

    -- Create index on UserId
    CREATE NONCLUSTERED INDEX [IX_Doctors_UserId] ON [dbo].[Doctors] ([UserId] ASC);

    PRINT '? Doctors table created successfully';
END
ELSE
BEGIN
    PRINT '? Doctors table already exists';
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
        [CreatedAt] datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
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
    CREATE NONCLUSTERED INDEX [IX_Reviews_ClientId] ON [dbo].[Reviews] ([ClientId] ASC);
    CREATE NONCLUSTERED INDEX [IX_Reviews_DoctorId] ON [dbo].[Reviews] ([DoctorId] ASC);
    CREATE NONCLUSTERED INDEX [IX_Reviews_AppointmentId] ON [dbo].[Reviews] ([AppointmentId] ASC);

    PRINT '? Reviews table created successfully';
END
ELSE
BEGIN
    PRINT '? Reviews table already exists';
END

-- Step 3: Handle Appointments table - check what exists and fix it
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Appointments]') AND type in (N'U'))
BEGIN
    PRINT '? Appointments table exists - checking structure...';
    
    -- Add missing columns if they don't exist
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Appointments') AND name = 'Reason')
    BEGIN
        ALTER TABLE [Appointments] ADD [Reason] nvarchar(max) NOT NULL DEFAULT '';
        PRINT '? Added Reason column to Appointments';
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Appointments') AND name = 'AppointmentTime')
    BEGIN
        ALTER TABLE [Appointments] ADD [AppointmentTime] time NOT NULL DEFAULT '00:00:00';
        PRINT '? Added AppointmentTime column to Appointments';
    END

    -- Check current DoctorId type
    DECLARE @CurrentDoctorIdType nvarchar(50);
    SELECT @CurrentDoctorIdType = t.name
    FROM sys.columns c
    INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
    WHERE c.object_id = OBJECT_ID('Appointments') AND c.name = 'DoctorId';

    PRINT 'Current DoctorId type: ' + ISNULL(@CurrentDoctorIdType, 'Column not found');

    -- Only fix DoctorId if it's not int
    IF @CurrentDoctorIdType != 'int' AND @CurrentDoctorIdType IS NOT NULL
    BEGIN
        PRINT 'Converting DoctorId from ' + @CurrentDoctorIdType + ' to int...';
        
        -- Step 3a: Drop foreign key constraints on DoctorId if they exist
        DECLARE @FK_Name nvarchar(128);
        SELECT @FK_Name = fk.name
        FROM sys.foreign_keys fk
        INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
        INNER JOIN sys.columns c ON fkc.parent_column_id = c.column_id AND fkc.parent_object_id = c.object_id
        WHERE fk.parent_object_id = OBJECT_ID('Appointments') AND c.name = 'DoctorId';

        IF @FK_Name IS NOT NULL
        BEGIN
            EXEC('ALTER TABLE [Appointments] DROP CONSTRAINT [' + @FK_Name + ']');
            PRINT '? Dropped foreign key constraint: ' + @FK_Name;
        END

        -- Step 3b: Drop index if exists
        IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('Appointments') AND name = 'IX_Appointments_DoctorId')
        BEGIN
            DROP INDEX [IX_Appointments_DoctorId] ON [Appointments];
            PRINT '? Dropped index IX_Appointments_DoctorId';
        END

        -- Step 3c: Handle the column conversion
        -- First, let's create a backup
        DECLARE @AppCount int;
        SELECT @AppCount = COUNT(*) FROM Appointments;
        
        IF @AppCount > 0
        BEGIN
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Appointments_Backup]'))
            BEGIN
                SELECT * INTO Appointments_Backup FROM Appointments;
                PRINT '? Created backup table with ' + CAST(@AppCount AS varchar(10)) + ' records';
            END
            
            -- For existing data, set all DoctorId to 1 (will need manual correction)
            ALTER TABLE [Appointments] ADD [DoctorId_Temp] int NOT NULL DEFAULT 1;
            ALTER TABLE [Appointments] DROP COLUMN [DoctorId];
            EXEC sp_rename 'Appointments.DoctorId_Temp', 'DoctorId', 'COLUMN';
            PRINT '? Converted DoctorId column to int (existing records set to 1)';
            PRINT '??  WARNING: All existing appointments now have DoctorId = 1';
            PRINT '   You will need to manually update these to correct doctor IDs';
        END
        ELSE
        BEGIN
            -- No data, safe to just alter the column
            ALTER TABLE [Appointments] DROP COLUMN [DoctorId];
            ALTER TABLE [Appointments] ADD [DoctorId] int NOT NULL DEFAULT 1;
            PRINT '? Converted DoctorId column to int (no data loss)';
        END
    END
    ELSE IF @CurrentDoctorIdType = 'int'
    BEGIN
        PRINT '? DoctorId is already int type';
    END
    ELSE
    BEGIN
        -- DoctorId column doesn't exist, add it
        ALTER TABLE [Appointments] ADD [DoctorId] int NOT NULL DEFAULT 1;
        PRINT '? Added DoctorId column as int';
    END
END
ELSE
BEGIN
    PRINT 'Creating Appointments table from scratch...';
    
    CREATE TABLE [Appointments] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [PatientId] nvarchar(450) NOT NULL,
        [DoctorId] int NOT NULL,
        [AppointmentDate] datetime2 NOT NULL,
        [AppointmentTime] time NOT NULL DEFAULT '00:00:00',
        [Reason] nvarchar(max) NOT NULL DEFAULT '',
        [Notes] nvarchar(max) NULL,
        [Status] int NOT NULL DEFAULT 0,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [CompletedAt] datetime2 NULL,
        CONSTRAINT [PK_Appointments] PRIMARY KEY ([Id])
    );
    
    PRINT '? Appointments table created from scratch';
END

-- Step 4: Create all the foreign key relationships
PRINT 'Creating foreign key relationships...';

-- FK between Appointments and Doctors
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Appointments_Doctors_DoctorId')
BEGIN
    -- Create index first if it doesn't exist
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('Appointments') AND name = 'IX_Appointments_DoctorId')
    BEGIN
        CREATE INDEX [IX_Appointments_DoctorId] ON [Appointments] ([DoctorId]);
        PRINT '? Created index IX_Appointments_DoctorId';
    END
    
    ALTER TABLE [Appointments] WITH CHECK ADD CONSTRAINT [FK_Appointments_Doctors_DoctorId] 
    FOREIGN KEY([DoctorId]) REFERENCES [Doctors] ([Id]);
    
    ALTER TABLE [Appointments] CHECK CONSTRAINT [FK_Appointments_Doctors_DoctorId];
    PRINT '? Created FK constraint: Appointments -> Doctors';
END

-- FK between Appointments and Users (PatientId)
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Appointments_AspNetUsers_PatientId')
BEGIN
    -- Create index first if it doesn't exist
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('Appointments') AND name = 'IX_Appointments_PatientId')
    BEGIN
        CREATE INDEX [IX_Appointments_PatientId] ON [Appointments] ([PatientId]);
        PRINT '? Created index IX_Appointments_PatientId';
    END
    
    ALTER TABLE [Appointments] WITH CHECK ADD CONSTRAINT [FK_Appointments_AspNetUsers_PatientId] 
    FOREIGN KEY([PatientId]) REFERENCES [AspNetUsers] ([Id]);
    
    ALTER TABLE [Appointments] CHECK CONSTRAINT [FK_Appointments_AspNetUsers_PatientId];
    PRINT '? Created FK constraint: Appointments -> Users (PatientId)';
END

-- FK between Reviews and Appointments (only if Appointments table has data structure ready)
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Reviews_Appointments_AppointmentId')
AND EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Appointments]'))
BEGIN
    ALTER TABLE [Reviews] WITH CHECK ADD CONSTRAINT [FK_Reviews_Appointments_AppointmentId] 
    FOREIGN KEY([AppointmentId]) REFERENCES [Appointments] ([Id]);
    
    ALTER TABLE [Reviews] CHECK CONSTRAINT [FK_Reviews_Appointments_AppointmentId];
    PRINT '? Created FK constraint: Reviews -> Appointments';
END

-- Step 5: Update migration history
IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20250104140000_CreateDoctorsTable')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250104140000_CreateDoctorsTable', N'6.0.0');
    PRINT '? Updated migration history';
END

-- Final verification
PRINT '';
PRINT '==================== FINAL VERIFICATION ====================';

-- Check all tables exist
SELECT 
    CASE WHEN EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Doctors]'))
         THEN '? Doctors table exists'
         ELSE '? Doctors table missing'
    END as DoctorsStatus
UNION ALL
SELECT 
    CASE WHEN EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Reviews]'))
         THEN '? Reviews table exists'
         ELSE '? Reviews table missing'
    END
UNION ALL
SELECT 
    CASE WHEN EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Appointments]'))
         THEN '? Appointments table exists'
         ELSE '? Appointments table missing'
    END;

-- Check DoctorId column type
DECLARE @FinalType nvarchar(50);
SELECT @FinalType = t.name
FROM sys.columns c
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID('Appointments') AND c.name = 'DoctorId';

IF @FinalType = 'int'
    PRINT '? DoctorId column type is correct (int)';
ELSE
    PRINT '? DoctorId column type is: ' + ISNULL(@FinalType, 'missing');

-- Show record counts
PRINT '';
PRINT 'Current record counts:';
EXEC('
SELECT ''Doctors'' as TableName, COUNT(*) as Records FROM Doctors
UNION ALL
SELECT ''Reviews'' as TableName, COUNT(*) as Records FROM Reviews  
UNION ALL
SELECT ''Appointments'' as TableName, COUNT(*) as Records FROM Appointments
');

PRINT '';
PRINT '?? Simple migration completed successfully!';
PRINT 'Next step: Run the InsertSampleDoctors.sql script to add sample data.';
PRINT '============================================================';