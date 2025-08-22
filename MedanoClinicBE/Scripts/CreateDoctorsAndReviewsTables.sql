-- Manual SQL Script for Adding Doctors and Reviews Tables
-- Execute this in SQL Server Management Studio or Azure Data Studio

-- First, add new columns to Appointments table
ALTER TABLE [Appointments] ADD [Reason] nvarchar(max) NOT NULL DEFAULT '';
ALTER TABLE [Appointments] ADD [AppointmentTime] time NOT NULL DEFAULT '00:00:00';

-- Change DoctorId from string to int (this might require data migration if you have existing data)
-- Note: If you have existing data, you'll need to handle this carefully
ALTER TABLE [Appointments] ALTER COLUMN [DoctorId] int NOT NULL;

-- Create Doctors table
CREATE TABLE [Doctors] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [UserId] nvarchar(450) NOT NULL,
    [Specialization] nvarchar(max) NOT NULL,
    [Phone] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Doctors] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Doctors_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
);

-- Create Reviews table
CREATE TABLE [Reviews] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [ClientId] nvarchar(450) NOT NULL,
    [DoctorId] int NOT NULL,
    [AppointmentId] int NOT NULL,
    [Rating] int NOT NULL,
    [Comment] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Reviews] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Reviews_Appointments_AppointmentId] FOREIGN KEY ([AppointmentId]) REFERENCES [Appointments] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Reviews_AspNetUsers_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Reviews_Doctors_DoctorId] FOREIGN KEY ([DoctorId]) REFERENCES [Doctors] ([Id]) ON DELETE NO ACTION
);

-- Create indexes
CREATE INDEX [IX_Doctors_UserId] ON [Doctors] ([UserId]);
CREATE INDEX [IX_Reviews_AppointmentId] ON [Reviews] ([AppointmentId]);
CREATE INDEX [IX_Reviews_ClientId] ON [Reviews] ([ClientId]);
CREATE INDEX [IX_Reviews_DoctorId] ON [Reviews] ([DoctorId]);

-- Update the migration history table to mark this migration as applied
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250104120000_AddDoctorReviewAndUpdateAppointment', N'6.0.0');