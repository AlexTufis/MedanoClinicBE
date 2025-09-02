-- Database Reset Script for MedanoClinic
-- Use this to completely reset your database and resolve migration conflicts

-- WARNING: This will delete ALL data in your database!
-- Make sure to backup important data before running this script

-- Step 1: Drop the existing database (replace 'YourDatabaseName' with your actual database name)
-- Uncomment the lines below and replace with your database name
-- USE master;
-- DROP DATABASE IF EXISTS [MedanoClinicDB]; -- Replace with your database name

-- Alternative: If you want to keep the database but reset tables
-- Run this section instead of dropping the entire database

-- Step 2: Drop all custom tables (keeping Identity tables)
IF OBJECT_ID('MedicalReports', 'U') IS NOT NULL
    DROP TABLE [MedicalReports];

IF OBJECT_ID('Reviews', 'U') IS NOT NULL
    DROP TABLE [Reviews];

IF OBJECT_ID('AppointmentHours', 'U') IS NOT NULL
    DROP TABLE [AppointmentHours];

IF OBJECT_ID('Appointments', 'U') IS NOT NULL
    DROP TABLE [Appointments];

IF OBJECT_ID('Doctors', 'U') IS NOT NULL
    DROP TABLE [Doctors];

-- Step 3: Clear migration history (optional - only if you want to re-run all migrations)
-- IF OBJECT_ID('__EFMigrationsHistory', 'U') IS NOT NULL
--     DELETE FROM [__EFMigrationsHistory] WHERE MigrationId LIKE '%CreateDoctorsTable%'
--        OR MigrationId LIKE '%CreateAppointmentsTable%'
--        OR MigrationId LIKE '%CreateReviewsTable%'
--        OR MigrationId LIKE '%CreateAppointmentHoursTable%'
--        OR MigrationId LIKE '%AddMedicalReportsTable%';

PRINT 'Database tables dropped successfully!';
PRINT 'Now run your application - EF migrations will recreate all tables in correct order.';

-- Step 4: Verify cleanup
SELECT 
    TABLE_NAME,
    TABLE_TYPE
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME IN ('Doctors', 'Appointments', 'Reviews', 'AppointmentHours', 'MedicalReports')
ORDER BY TABLE_NAME;

PRINT 'If the query above returns no results, cleanup was successful!';