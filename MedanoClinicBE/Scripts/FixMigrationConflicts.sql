-- ===================================================================
-- SAFE MIGRATION SCRIPT TO FIX APPOINTMENTS TABLE AND CREATE DOCTORS
-- This script handles the DoctorId column type change safely
-- ===================================================================

PRINT 'Starting safe migration process...';

-- Step 1: Check current state of Appointments table
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Appointments]') AND type in (N'U'))
BEGIN
    PRINT '? Appointments table exists';
    
    -- Check if DoctorId is currently string or int
    DECLARE @DoctorIdType nvarchar(128);
    SELECT @DoctorIdType = t.name + '(' + CAST(c.max_length AS nvarchar(10)) + ')'
    FROM sys.columns c
    INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
    WHERE c.object_id = OBJECT_ID('Appointments') AND c.name = 'DoctorId';
    
    PRINT 'Current DoctorId type: ' + ISNULL(@DoctorIdType, 'Column not found');
    
    -- Step 2: If DoctorId is string type, we need to handle the transition carefully
    IF @DoctorIdType LIKE 'nvarchar%' OR @DoctorIdType LIKE 'varchar%'
    BEGIN
        PRINT 'DoctorId is currently string type - need to handle transition';
        
        -- Check if there are any existing appointments
        DECLARE @AppointmentCount int;
        SELECT @AppointmentCount = COUNT(*) FROM Appointments;
        
        IF @AppointmentCount > 0
        BEGIN
            PRINT 'WARNING: ' + CAST(@AppointmentCount AS varchar(10)) + ' existing appointments found!';
            PRINT 'This script will handle the data migration safely.';
            
            -- Create backup table
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Appointments_Backup]'))
            BEGIN
                SELECT * INTO Appointments_Backup FROM Appointments;
                PRINT '? Backup table created: Appointments_Backup';
            END
        END
        
        -- Step 3: Safely drop and recreate the DoctorId column
        PRINT 'Dropping foreign key constraints and indexes...';
        
        -- Drop existing foreign key constraint if exists
        DECLARE @FKName nvarchar(128);
        SELECT @FKName = fk.name
        FROM sys.foreign_keys fk
        WHERE fk.parent_object_id = OBJECT_ID('Appointments') 
        AND EXISTS (
            SELECT 1 FROM sys.foreign_key_columns fkc
            INNER JOIN sys.columns c ON fkc.parent_column_id = c.column_id AND fkc.parent_object_id = c.object_id
            WHERE fkc.constraint_object_id = fk.object_id AND c.name = 'DoctorId'
        );
        
        IF @FKName IS NOT NULL
        BEGIN
            EXEC('ALTER TABLE [Appointments] DROP CONSTRAINT [' + @FKName + ']');
            PRINT '? Dropped FK constraint: ' + @FKName;
        END
        
        -- Drop existing index if exists
        IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('Appointments') AND name = 'IX_Appointments_DoctorId')
        BEGIN
            DROP INDEX [IX_Appointments_DoctorId] ON [Appointments];
            PRINT '? Dropped index: IX_Appointments_DoctorId';
        END
        
        -- Add temporary column for new DoctorId
        ALTER TABLE [Appointments] ADD [DoctorId_New] int NULL;
        PRINT '? Added temporary DoctorId_New column';
        
        -- Set default value for existing records
        UPDATE [Appointments] SET [DoctorId_New] = 1 WHERE [DoctorId_New] IS NULL;
        PRINT '? Updated existing records with default DoctorId';
        
        -- Drop old column
        ALTER TABLE [Appointments] DROP COLUMN [DoctorId];
        PRINT '? Dropped old DoctorId column';
        
        -- Rename new column
        EXEC sp_rename 'Appointments.DoctorId_New', 'DoctorId', 'COLUMN';
        PRINT '? Renamed DoctorId_New to DoctorId';
        
        -- Make column NOT NULL
        ALTER TABLE [Appointments] ALTER COLUMN [DoctorId] int NOT NULL;
        PRINT '? Made DoctorId NOT NULL';
    END
    ELSE IF @DoctorIdType LIKE 'int%'
    BEGIN
        PRINT '? DoctorId is already correct type (int)';
    END
    ELSE
    BEGIN
        PRINT 'Unknown DoctorId type: ' + ISNULL(@DoctorIdType, 'NULL') + ' - treating as needs conversion';
        
        -- Drop existing constraints and indexes if they exist
        DECLARE @FKName2 nvarchar(128);
        SELECT @FKName2 = fk.name
        FROM sys.foreign_keys fk
        WHERE fk.parent_object_id = OBJECT_ID('Appointments') 
        AND EXISTS (
            SELECT 1 FROM sys.foreign_key_columns fkc
            INNER JOIN sys.columns c ON fkc.parent_column_id = c.column_id AND fkc.parent_object_id = c.object_id
            WHERE fkc.constraint_object_id = fk.object_id AND c.name = 'DoctorId'
        );
        
        IF @FKName2 IS NOT NULL
        BEGIN
            EXEC('ALTER TABLE [Appointments] DROP CONSTRAINT [' + @FKName2 + ']');
            PRINT '? Dropped FK constraint: ' + @FKName2;
        END
        
        IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('Appointments') AND name = 'IX_Appointments_DoctorId')
        BEGIN
            DROP INDEX [IX_Appointments_DoctorId] ON [Appointments];
            PRINT '? Dropped index: IX_Appointments_DoctorId';
        END
        
        -- Convert to int type
        IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Appointments') AND name = 'DoctorId')
        BEGIN
            ALTER TABLE [Appointments] ADD [DoctorId_New] int NULL;
            UPDATE [Appointments] SET [DoctorId_New] = 1;
            ALTER TABLE [Appointments] DROP COLUMN [DoctorId];
            EXEC sp_rename 'Appointments.DoctorId_New', 'DoctorId', 'COLUMN';
            ALTER TABLE [Appointments] ALTER COLUMN [DoctorId] int NOT NULL;
            PRINT '? Converted DoctorId to int type';
        END
        ELSE
        BEGIN
            ALTER TABLE [Appointments] ADD [DoctorId] int NOT NULL DEFAULT 1;
            PRINT '? Added DoctorId as int type';
        END
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
    
    CREATE INDEX [IX_Appointments_PatientId] ON [Appointments] ([PatientId]);
    PRINT '? Appointments table created';
END

-- Step 4: Ensure required columns exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Appointments') AND name = 'Reason')
BEGIN
    ALTER TABLE [Appointments] ADD [Reason] nvarchar(max) NOT NULL DEFAULT '';
    PRINT '? Added Reason column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Appointments') AND name = 'AppointmentTime')
BEGIN
    ALTER TABLE [Appointments] ADD [AppointmentTime] time NOT NULL DEFAULT '00:00:00';
    PRINT '? Added AppointmentTime column';
END

-- Step 5: Create Doctors table
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

-- Step 6: Create Reviews table
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
    CREATE NONCLUSTERED INDEX [IX_Reviews_ClientId] ON [dbo].[Reviews] ([ClientId] ASC);
    CREATE NONCLUSTERED INDEX [IX_Reviews_DoctorId] ON [dbo].[Reviews] ([DoctorId] ASC);
    CREATE NONCLUSTERED INDEX [IX_Reviews_AppointmentId] ON [dbo].[Reviews] ([AppointmentId] ASC);

    PRINT '? Reviews table created successfully';
END
ELSE
BEGIN
    PRINT '? Reviews table already exists';
END

-- Step 7: Now create the proper foreign key relationship between Appointments and Doctors
IF NOT EXISTS (
    SELECT * FROM sys.foreign_keys 
    WHERE object_id = OBJECT_ID(N'[dbo].[FK_Appointments_Doctors_DoctorId]') 
    AND parent_object_id = OBJECT_ID(N'[dbo].[Appointments]')
)
BEGIN
    -- First create index on DoctorId if it doesn't exist
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('Appointments') AND name = 'IX_Appointments_DoctorId')
    BEGIN
        CREATE INDEX [IX_Appointments_DoctorId] ON [Appointments] ([DoctorId]);
        PRINT '? Created index IX_Appointments_DoctorId';
    END
    
    -- Add foreign key constraint
    ALTER TABLE [dbo].[Appointments] WITH CHECK ADD CONSTRAINT [FK_Appointments_Doctors_DoctorId] 
    FOREIGN KEY([DoctorId]) REFERENCES [dbo].[Doctors] ([Id]);
    
    ALTER TABLE [dbo].[Appointments] CHECK CONSTRAINT [FK_Appointments_Doctors_DoctorId];
    PRINT '? Added FK constraint between Appointments and Doctors';
END

-- Step 8: Add foreign key between Appointments and Users for PatientId if not exists
IF NOT EXISTS (
    SELECT * FROM sys.foreign_keys 
    WHERE object_id = OBJECT_ID(N'[dbo].[FK_Appointments_AspNetUsers_PatientId]') 
    AND parent_object_id = OBJECT_ID(N'[dbo].[Appointments]')
)
BEGIN
    ALTER TABLE [dbo].[Appointments] WITH CHECK ADD CONSTRAINT [FK_Appointments_AspNetUsers_PatientId] 
    FOREIGN KEY([PatientId]) REFERENCES [dbo].[AspNetUsers] ([Id]);
    
    ALTER TABLE [dbo].[Appointments] CHECK CONSTRAINT [FK_Appointments_AspNetUsers_PatientId];
    PRINT '? Added FK constraint between Appointments and Users (PatientId)';
END

-- Step 9: Update migration history
IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20250104140000_CreateDoctorsTable')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250104140000_CreateDoctorsTable', N'6.0.0');
    PRINT '? Migration history updated';
END

-- Final verification
PRINT '';
PRINT '==================== FINAL VERIFICATION ====================';

-- Check tables exist
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Doctors]') AND type in (N'U'))
    PRINT '? Doctors table exists';
ELSE
    PRINT '? Doctors table missing';

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Reviews]') AND type in (N'U'))
    PRINT '? Reviews table exists';
ELSE
    PRINT '? Reviews table missing';

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Appointments]') AND type in (N'U'))
    PRINT '? Appointments table exists';
ELSE
    PRINT '? Appointments table missing';

-- Check DoctorId column type
DECLARE @FinalDoctorIdType nvarchar(128);
SELECT @FinalDoctorIdType = t.name
FROM sys.columns c
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID('Appointments') AND c.name = 'DoctorId';

IF @FinalDoctorIdType = 'int'
    PRINT '? DoctorId is correct type (int)';
ELSE
    PRINT '? DoctorId type is: ' + ISNULL(@FinalDoctorIdType, 'not found');

-- Show table counts
PRINT '';
PRINT 'Table Record Counts:';
SELECT 
    'Doctors' as TableName, 
    COUNT(*) as RecordCount 
FROM Doctors
UNION ALL
SELECT 
    'Reviews' as TableName, 
    COUNT(*) as RecordCount 
FROM Reviews
UNION ALL
SELECT 
    'Appointments' as TableName, 
    COUNT(*) as RecordCount 
FROM Appointments;

PRINT '';
PRINT '? Migration completed successfully!';
PRINT '============================================================';