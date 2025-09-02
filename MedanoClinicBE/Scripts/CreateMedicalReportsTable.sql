-- Script to create MedicalReports table for MedanoClinic
-- Run this directly in SQL Server Management Studio or Azure Data Studio

-- Create the MedicalReports table with Romanian medical terminology
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'MedicalReports')
BEGIN
    CREATE TABLE [MedicalReports] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [AppointmentId] int NOT NULL,
        [DoctorId] int NOT NULL,
        [PatientId] nvarchar(450) NOT NULL,
        
        -- Romanian medical fields
        [Antecedente] nvarchar(max) NULL,      -- Medical history
        [Simptome] nvarchar(max) NULL,         -- Symptoms
        [Clinice] nvarchar(max) NULL,          -- Clinical examination findings
        [Paraclinice] nvarchar(max) NULL,      -- Laboratory/imaging test results
        [Diagnostic] nvarchar(max) NULL,       -- Medical diagnosis
        [Recomandari] nvarchar(max) NULL,      -- Treatment recommendations
        
        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] datetime2 NULL,
        
        -- Primary Key
        CONSTRAINT [PK_MedicalReports] PRIMARY KEY ([Id])
    );
    
    -- Create indexes for better query performance
    CREATE INDEX [IX_MedicalReports_AppointmentId] ON [MedicalReports] ([AppointmentId]);
    CREATE INDEX [IX_MedicalReports_DoctorId] ON [MedicalReports] ([DoctorId]);
    CREATE INDEX [IX_MedicalReports_PatientId] ON [MedicalReports] ([PatientId]);
    
    -- Create unique index to ensure one report per appointment
    CREATE UNIQUE INDEX [IX_MedicalReports_AppointmentId_Unique] ON [MedicalReports] ([AppointmentId]);
    
    PRINT 'MedicalReports table created successfully with indexes';
END
ELSE
BEGIN
    PRINT 'MedicalReports table already exists';
END
GO

-- Add foreign key constraints (run after table creation)
-- FK to Appointments table
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_MedicalReports_Appointments_AppointmentId')
BEGIN
    ALTER TABLE [MedicalReports] 
    ADD CONSTRAINT [FK_MedicalReports_Appointments_AppointmentId] 
    FOREIGN KEY ([AppointmentId]) REFERENCES [Appointments] ([Id]) ON DELETE NO ACTION;
    
    PRINT 'Added foreign key constraint to Appointments table';
END

-- FK to Doctors table
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_MedicalReports_Doctors_DoctorId')
BEGIN
    ALTER TABLE [MedicalReports] 
    ADD CONSTRAINT [FK_MedicalReports_Doctors_DoctorId] 
    FOREIGN KEY ([DoctorId]) REFERENCES [Doctors] ([Id]) ON DELETE NO ACTION;
    
    PRINT 'Added foreign key constraint to Doctors table';
END

-- FK to AspNetUsers table (for Patient)
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_MedicalReports_AspNetUsers_PatientId')
BEGIN
    ALTER TABLE [MedicalReports] 
    ADD CONSTRAINT [FK_MedicalReports_AspNetUsers_PatientId] 
    FOREIGN KEY ([PatientId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION;
    
    PRINT 'Added foreign key constraint to AspNetUsers table';
END

-- Verify table creation
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'MedicalReports')
BEGIN
    PRINT '? MedicalReports table successfully created and configured!';
    
    -- Display table structure
    SELECT 
        COLUMN_NAME as 'Column Name',
        DATA_TYPE as 'Data Type',
        IS_NULLABLE as 'Nullable',
        COLUMN_DEFAULT as 'Default Value'
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'MedicalReports'
    ORDER BY ORDINAL_POSITION;
    
    -- Display indexes
    SELECT 
        i.name as 'Index Name',
        i.is_unique as 'Is Unique',
        c.name as 'Column Name'
    FROM sys.indexes i
    INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
    INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
    WHERE i.object_id = OBJECT_ID('MedicalReports')
    ORDER BY i.name, ic.key_ordinal;
    
    -- Display foreign keys
    SELECT 
        fk.name as 'Foreign Key Name',
        tp.name as 'Parent Table',
        cp.name as 'Parent Column',
        tr.name as 'Referenced Table',
        cr.name as 'Referenced Column'
    FROM sys.foreign_keys fk
    INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
    INNER JOIN sys.tables tp ON fkc.parent_object_id = tp.object_id
    INNER JOIN sys.columns cp ON fkc.parent_object_id = cp.object_id AND fkc.parent_column_id = cp.column_id
    INNER JOIN sys.tables tr ON fkc.referenced_object_id = tr.object_id
    INNER JOIN sys.columns cr ON fkc.referenced_object_id = cr.object_id AND fkc.referenced_column_id = cr.column_id
    WHERE tp.name = 'MedicalReports';
END
ELSE
BEGIN
    PRINT '? Failed to create MedicalReports table';
END