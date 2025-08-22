using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedanoClinicBE.Migrations
{
    /// <inheritdoc />
    public partial class CreateDoctorsTableClean : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // This migration uses raw SQL to avoid EF conflicts
            migrationBuilder.Sql(@"
                -- Create Doctors table if it doesn't exist
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Doctors' AND xtype='U')
                BEGIN
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
                    
                    CREATE INDEX [IX_Doctors_UserId] ON [Doctors] ([UserId]);
                END

                -- Create Reviews table if it doesn't exist
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Reviews' AND xtype='U')
                BEGIN
                    CREATE TABLE [Reviews] (
                        [Id] int IDENTITY(1,1) NOT NULL,
                        [ClientId] nvarchar(450) NOT NULL,
                        [DoctorId] int NOT NULL,
                        [AppointmentId] int NOT NULL,
                        [Rating] int NOT NULL,
                        [Comment] nvarchar(max) NULL,
                        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
                        CONSTRAINT [PK_Reviews] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_Reviews_AspNetUsers_ClientId] FOREIGN KEY ([ClientId]) 
                            REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
                        CONSTRAINT [FK_Reviews_Doctors_DoctorId] FOREIGN KEY ([DoctorId]) 
                            REFERENCES [Doctors] ([Id]) ON DELETE NO ACTION
                    );
                    
                    CREATE INDEX [IX_Reviews_ClientId] ON [Reviews] ([ClientId]);
                    CREATE INDEX [IX_Reviews_DoctorId] ON [Reviews] ([DoctorId]);
                    CREATE INDEX [IX_Reviews_AppointmentId] ON [Reviews] ([AppointmentId]);
                END

                -- Handle Appointments table modifications safely
                -- Add missing columns if they don't exist
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Appointments') AND name = 'Reason')
                BEGIN
                    ALTER TABLE [Appointments] ADD [Reason] nvarchar(max) NOT NULL DEFAULT '';
                END

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Appointments') AND name = 'AppointmentTime')
                BEGIN
                    ALTER TABLE [Appointments] ADD [AppointmentTime] time NOT NULL DEFAULT '00:00:00';
                END

                -- Only modify DoctorId if it's currently string type
                DECLARE @DoctorIdType nvarchar(128);
                SELECT @DoctorIdType = t.name
                FROM sys.columns c
                INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
                WHERE c.object_id = OBJECT_ID('Appointments') AND c.name = 'DoctorId';
                
                IF @DoctorIdType IN ('nvarchar', 'varchar', 'nchar', 'char')
                BEGIN
                    -- Handle the string to int conversion safely
                    -- Drop existing constraints and indexes
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
                        EXEC('ALTER TABLE [Appointments] DROP CONSTRAINT [' + @FKName + ']');
                    
                    IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('Appointments') AND name = 'IX_Appointments_DoctorId')
                        DROP INDEX [IX_Appointments_DoctorId] ON [Appointments];
                    
                    -- Add temporary column, update data, drop old column, rename new column
                    ALTER TABLE [Appointments] ADD [DoctorId_New] int NULL;
                    UPDATE [Appointments] SET [DoctorId_New] = 1 WHERE [DoctorId_New] IS NULL;
                    ALTER TABLE [Appointments] DROP COLUMN [DoctorId];
                    EXEC sp_rename 'Appointments.DoctorId_New', 'DoctorId', 'COLUMN';
                    ALTER TABLE [Appointments] ALTER COLUMN [DoctorId] int NOT NULL;
                END

                -- Create proper foreign key relationships
                IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'FK_Appointments_Doctors_DoctorId'))
                BEGIN
                    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('Appointments') AND name = 'IX_Appointments_DoctorId')
                        CREATE INDEX [IX_Appointments_DoctorId] ON [Appointments] ([DoctorId]);
                    
                    ALTER TABLE [Appointments] WITH CHECK ADD CONSTRAINT [FK_Appointments_Doctors_DoctorId] 
                    FOREIGN KEY([DoctorId]) REFERENCES [Doctors] ([Id]);
                    
                    ALTER TABLE [Appointments] CHECK CONSTRAINT [FK_Appointments_Doctors_DoctorId];
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Reviews");
            migrationBuilder.DropTable(name: "Doctors");
        }
    }
}