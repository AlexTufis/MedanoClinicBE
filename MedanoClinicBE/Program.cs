using MedanoClinicBE.Data;
using MedanoClinicBE.Helpers;
using MedanoClinicBE.Models;
using MedanoClinicBE.Repositories;
using MedanoClinicBE.Repositories.Interfaces;
using MedanoClinicBE.Services;
using MedanoClinicBE.Services.Interfaces;
using MedanoClinicBE.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Hangfire;
using Hangfire.SqlServer;
using Hangfire.Dashboard;

var builder = WebApplication.CreateBuilder(args);

// 1. EF + Identity
builder.Services.AddDbContext<ApplicationDbContext>(opts =>
  opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
  .AddIdentity<ApplicationUser, IdentityRole>(options =>
  {
      // Configure password requirements for easier testing
      options.Password.RequireDigit = false;
      options.Password.RequiredLength = 6;
      options.Password.RequireNonAlphanumeric = false;
      options.Password.RequireUppercase = false;
      options.Password.RequireLowercase = false;
  })
  .AddEntityFrameworkStores<ApplicationDbContext>()
  .AddDefaultTokenProviders();

// 2. JWT Authentication
var jwtOpts = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
builder.Services.AddSingleton(jwtOpts);

builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(opts =>
{
    opts.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtOpts.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtOpts.Audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOpts.Secret)),
        ValidateLifetime = true
    };
});

// 3. Email Settings Configuration
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// 4. Hangfire Configuration
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true
    }));

// Add Hangfire server
builder.Services.AddHangfireServer(options =>
{
    options.SchedulePollingInterval = TimeSpan.FromSeconds(30);
    options.Queues = new[] { "notifications", "default" };
});

// 5. Repository and Service Registration
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IClientService, ClientService>();

// 6. Notification Services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IJobService, JobService>();

// 7. CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 8. Enhanced Database Initialization
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("Starting database initialization...");
        
        // Apply any pending migrations
        var pendingMigrations = context.Database.GetPendingMigrations();
        if (pendingMigrations.Any())
        {
            logger.LogInformation($"Applying {pendingMigrations.Count()} pending migrations...");
            context.Database.Migrate();
            logger.LogInformation("Migrations applied successfully.");
        }
        else
        {
            logger.LogInformation("No pending migrations found.");
        }

        // Verify tables exist and create them if they don't
        await EnsureTablesExist(context, logger);
        
        logger.LogInformation("Database initialization completed successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred during database initialization.");
        
        // Fallback: Try to ensure database is created
        try
        {
            context.Database.EnsureCreated();
            logger.LogInformation("Fallback: Database created using EnsureCreated.");
        }
        catch (Exception fallbackEx)
        {
            logger.LogError(fallbackEx, "Fallback database creation also failed.");
        }
    }
    
    // 9. Seed Roles
    try
    {
        var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { "Admin", "Client", "Doctor" })
        {
            if (!await roleMgr.RoleExistsAsync(role))
            {
                await roleMgr.CreateAsync(new IdentityRole(role));
                logger.LogInformation($"Created role: {role}");
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while seeding roles.");
    }
}

// Helper method to ensure tables exist
static async Task EnsureTablesExist(ApplicationDbContext context, ILogger logger)
{
    try
    {
        logger.LogInformation("Checking database state and ensuring tables exist...");

        // Execute the safe migration script
        await context.Database.ExecuteSqlRawAsync(@"
            -- Check if Doctors table exists, create if not
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
        ");

        await context.Database.ExecuteSqlRawAsync(@"
            -- Check if Reviews table exists, create if not
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
        ");

        // Handle Appointments table modifications safely
        await context.Database.ExecuteSqlRawAsync(@"
            -- Add missing columns to Appointments if they don't exist
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Appointments') AND name = 'Reason')
            BEGIN
                ALTER TABLE [Appointments] ADD [Reason] nvarchar(max) NOT NULL DEFAULT '';
            END

            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Appointments') AND name = 'AppointmentTime')
            BEGIN
                ALTER TABLE [Appointments] ADD [AppointmentTime] time NOT NULL DEFAULT '00:00:00';
            END
        ");

        // Handle DoctorId type conversion if needed
        await context.Database.ExecuteSqlRawAsync(@"
            -- Check if DoctorId needs to be converted from string to int
            DECLARE @DoctorIdType nvarchar(128);
            SELECT @DoctorIdType = t.name
            FROM sys.columns c
            INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
            WHERE c.object_id = OBJECT_ID('Appointments') AND c.name = 'DoctorId';
            
            IF @DoctorIdType IN ('nvarchar', 'varchar', 'nchar', 'char')
            BEGIN
                -- Convert DoctorId from string to int safely
                DECLARE @FKName nvarchar(128);
                SELECT @FKName = fk.name
                FROM sys.foreign_keys fk
                WHERE fk.parent_object_id = OBJECT_ID('Appointments') 
                AND EXISTS (
                    SELECT 1 FROM sys.foreign_key_columns fkc
                    INNER JOIN sys.columns c ON fkc.parent_column_id = c.column_id AND fkc.parent_object_id = c.object_id
                    WHERE fkc.constraint_object_id = fk.object_id AND c.name = 'DoctorId'
                );
                
                -- Drop constraints and indexes
                IF @FKName IS NOT NULL
                    EXEC('ALTER TABLE [Appointments] DROP CONSTRAINT [' + @FKName + ']');
                
                IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('Appointments') AND name = 'IX_Appointments_DoctorId')
                    DROP INDEX [IX_Appointments_DoctorId] ON [Appointments];
                
                -- Convert column type
                ALTER TABLE [Appointments] ADD [DoctorId_New] int NULL;
                UPDATE [Appointments] SET [DoctorId_New] = 1 WHERE [DoctorId_New] IS NULL;
                ALTER TABLE [Appointments] DROP COLUMN [DoctorId];
                EXEC sp_rename 'Appointments.DoctorId_New', 'DoctorId', 'COLUMN';
                ALTER TABLE [Appointments] ALTER COLUMN [DoctorId] int NOT NULL;
            END
        ");

        // Create proper foreign key relationships
        await context.Database.ExecuteSqlRawAsync(@"
            -- Create foreign key between Appointments and Doctors
            IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'FK_Appointments_Doctors_DoctorId'))
            BEGIN
                -- Create index first if it doesn't exist
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('Appointments') AND name = 'IX_Appointments_DoctorId')
                    CREATE INDEX [IX_Appointments_DoctorId] ON [Appointments] ([DoctorId]);
                
                ALTER TABLE [Appointments] WITH CHECK ADD CONSTRAINT [FK_Appointments_Doctors_DoctorId] 
                FOREIGN KEY([DoctorId]) REFERENCES [Doctors] ([Id]);
                
                ALTER TABLE [Appointments] CHECK CONSTRAINT [FK_Appointments_Doctors_DoctorId];
            END

            -- Create foreign key between Appointments and Users (PatientId)
            IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'FK_Appointments_AspNetUsers_PatientId'))
            BEGIN
                ALTER TABLE [Appointments] WITH CHECK ADD CONSTRAINT [FK_Appointments_AspNetUsers_PatientId] 
                FOREIGN KEY([PatientId]) REFERENCES [AspNetUsers] ([Id]);
                
                ALTER TABLE [Appointments] CHECK CONSTRAINT [FK_Appointments_AspNetUsers_PatientId];
            END
        ");

        logger.LogInformation("Table existence verification and migration completed successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error occurred while ensuring tables exist: {ErrorMessage}", ex.Message);
        throw;
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    
    // Add Hangfire Dashboard for development (allows anonymous access in dev)
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        DashboardTitle = "MedanoClinic Job Dashboard"
        // In development, dashboard is accessible to localhost by default
    });
}

app.UseHttpsRedirection();

// Use CORS before authentication and authorization
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
