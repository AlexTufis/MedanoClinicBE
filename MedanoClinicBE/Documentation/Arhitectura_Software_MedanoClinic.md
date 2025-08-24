# ARHITECTURA SOFTWARE A APLICAȚIEI MEDANOCLINIC
## DOCUMENTAȚIE TEHNICĂ PENTRU LUCRAREA DE DISERTAȚIE

---

## CUPRINS

1. [Introducere în Arhitectura Software](#1-introducere-în-arhitectura-software)
2. [Principiile Arhitecturale Fundamentale](#2-principiile-arhitecturale-fundamentale)
3. [Stratificarea Aplicației](#3-stratificarea-aplicației)
4. [Componenta de Date (Data Layer)](#4-componenta-de-date-data-layer)
5. [Stratul de Acces la Date (Repository Layer)](#5-stratul-de-acces-la-date-repository-layer)
6. [Stratul de Logică de Business (Service Layer)](#6-stratul-de-logică-de-business-service-layer)
7. [Stratul de Prezentare (Presentation Layer)](#7-stratul-de-prezentare-presentation-layer)
8. [Sistemul de Securitate și Autentificare](#8-sistemul-de-securitate-și-autentificare)
9. [Infrastructura și Servicii Transversale](#9-infrastructura-și-servicii-transversale)
10. [Dependențele și Injecția de Dependențe](#10-dependențele-și-injecția-de-dependențe)
11. [Gestionarea Configurărilor](#11-gestionarea-configurărilor)
12. [Patternuri de Design Implementate](#12-patternuri-de-design-implementate)

---

## 1. INTRODUCERE ÎN ARHITECTURA SOFTWARE

### 1.1 Viziunea Arhitecturală

Sistemul **MedanoClinic** implementează o arhitectură **Clean Architecture** (arhitectura curată), inspirată din principiile lui Robert C. Martin (Uncle Bob). Această abordare asigură:

- **Independența framework-urilor**: Arhitectura nu depinde de existența unor biblioteci externe
- **Testabilitatea**: Logica de business poate fi testată independent de UI, baza de date sau serviciile externe
- **Independența UI**: Interfața utilizator poate fi schimbată fără a modifica restul sistemului
- **Independența bazei de date**: Sistemul nu este legat de un anumit tip de bază de date
- **Independența serviciilor externe**: Logica de business nu știe nimic despre lumea exterioară

### 1.2 Avantajele Arhitecturale

```
🏗️ BENEFICII ARHITECTURALE
├── Mentenabilitate ridicată prin separarea responsabilităților
├── Scalabilitate orizontală și verticală
├── Testabilitate completă la toate nivelurile
├── Flexibilitate în schimbarea tehnologiilor
└── Respectarea principiilor SOLID
```

### 1.3 Structura Generală a Proiectului

```
MedanoClinicBE/
├── 📁 Controllers/              # Presentation Layer - API Controllers
├── 📁 Services/                 # Business Logic Layer
│   ├── 📁 Interfaces/          # Contracte pentru servicii
│   ├── AdminService.cs         # Logică administrativă
│   ├── ClientService.cs        # Logică pentru pacienți
│   ├── EmailService.cs         # Servicii de comunicare
│   ├── JobService.cs           # Automatizare cu Hangfire
│   └── NotificationService.cs  # Gestionarea notificărilor
├── 📁 Repositories/            # Data Access Layer
│   ├── 📁 Interfaces/          # Contracte pentru repositrii
│   ├── AppointmentRepository.cs
│   ├── DoctorRepository.cs
│   ├── ReviewRepository.cs
│   └── UserRepository.cs
├── 📁 Models/                  # Domain Entities
│   ├── ApplicationUser.cs      # Entitatea utilizator
│   ├── Appointment.cs          # Entitatea programare
│   ├── Doctor.cs               # Entitatea doctor
│   └── Review.cs               # Entitatea review
├── 📁 DTOs/                    # Data Transfer Objects
├── 📁 Data/                    # Database Context
├── 📁 Settings/                # Configuration Classes
├── 📁 Helpers/                 # Utility Classes
└── 📁 Migrations/              # Database Migrations
```

---

## 2. PRINCIPIILE ARHITECTURALE FUNDAMENTALE

### 2.1 Dependența Inversă (Dependency Inversion Principle)

Sistemul respectă principiul că **modulele de nivel înalt nu trebuie să depindă de modulele de nivel jos**. Ambele trebuie să depindă de abstracții.

```csharp
// Exemplu: ClientService depinde de abstracții, nu de implementări concrete
public class ClientService : IClientService
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly IJobService _jobService;

    // Toate dependențele sunt injectate prin constructor
    public ClientService(
        IDoctorRepository doctorRepository, 
        IAppointmentRepository appointmentRepository, 
        IReviewRepository reviewRepository,
        IJobService jobService)
    {
        _doctorRepository = doctorRepository;
        _appointmentRepository = appointmentRepository;
        _reviewRepository = reviewRepository;
        _jobService = jobService;
    }
}
```

### 2.2 Separarea Responsabilităților (Separation of Concerns)

Fiecare componentă a sistemului are o responsabilitate specifică și bine definită:

#### 2.2.1 Responsabilități prin Straturi

| Strat | Responsabilitate | Exemple |
|-------|------------------|---------|
| **Controllers** | Gestionarea request-urilor HTTP și validare input | `AuthController`, `ClientController` |
| **Services** | Logica de business și orchestrarea operațiilor | `ClientService`, `AdminService` |
| **Repositories** | Accesul la date și persistența | `AppointmentRepository`, `DoctorRepository` |
| **Models** | Definirea entităților de domeniu | `Appointment`, `Doctor`, `Review` |

### 2.3 Principiul Deschis/Închis (Open/Closed Principle)

Sistemul este deschis pentru extindere dar închis pentru modificare prin utilizarea interfețelor:

```csharp
// Interface pentru serviciul de email - poate fi extins cu noi implementări
public interface IEmailService
{
    Task<bool> SendEmailAsync(EmailNotificationDto emailDto);
    Task<bool> SendAppointmentCreatedEmailAsync(AppointmentResponseDto appointment);
    Task<bool> SendAppointmentModifiedEmailAsync(AppointmentResponseDto appointment);
    Task<bool> SendAppointmentReminderEmailAsync(AppointmentResponseDto appointment);
    Task<bool> SendAppointmentCancelledEmailAsync(AppointmentResponseDto appointment);
}
```

---

## 3. STRATIFICAREA APLICAȚIEI

### 3.1 Diagrama Arhitecturală Generală

```
┌─────────────────────────────────────────────────────────┐
│                 PRESENTATION LAYER                       │
│  Controllers (HTTP Endpoints) + DTOs (Data Transfer)    │
└─────────────────┬───────────────────────────────────────┘
                  │ HTTP Requests/Responses
┌─────────────────▼───────────────────────────────────────┐
│                 BUSINESS LOGIC LAYER                     │
│     Services (Business Rules + Orchestration)          │
└─────────────────┬───────────────────────────────────────┘
                  │ Domain Operations
┌─────────────────▼───────────────────────────────────────┐
│                 DATA ACCESS LAYER                        │
│        Repositories (Data Operations)                   │
└─────────────────┬───────────────────────────────────────┘
                  │ Entity Framework Core
┌─────────────────▼───────────────────────────────────────┐
│                 DATABASE LAYER                           │
│     SQL Server + Entity Framework + Migrations         │
└─────────────────────────────────────────────────────────┘
```

### 3.2 Fluxul de Date în Arhitectură

#### 3.2.1 Fluxul pentru o Operațiune de Creare Programare

```
1. Client Request (HTTP POST) → AuthController
2. AuthController → ClientService.CreateAppointmentAsync()
3. ClientService → AppointmentRepository.CreateAppointmentAsync()
4. AppointmentRepository → ApplicationDbContext (Entity Framework)
5. ApplicationDbContext → SQL Server Database
6. Response înapoi prin același lanț cu AppointmentResponseDto
7. ClientService → JobService (Hangfire pentru notificări)
```

#### 3.2.2 Exemplu de Cod pentru Fluxul Complet

```csharp
// 1. Controller Layer - Primește request-ul
[HttpPost("appointments")]
[Authorize(Roles = "Client")]
public async Task<ActionResult<AppointmentResponseDto>> CreateAppointment(CreateAppointmentDto dto)
{
    var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    var appointment = await _clientService.CreateAppointmentAsync(dto, clientId);
    return Ok(appointment);
}

// 2. Service Layer - Orchestrează operațiunea
public async Task<AppointmentResponseDto> CreateAppointmentAsync(CreateAppointmentDto dto, string clientId)
{
    // Crează programarea
    var appointment = await _appointmentRepository.CreateAppointmentAsync(dto, clientId);
    
    // Declanșează notificări asincrone
    _jobService.SendAppointmentCreatedEmailJob(appointment);
    _jobService.ScheduleAppointmentReminder(appointment);
    
    return appointment;
}

// 3. Repository Layer - Accesează datele
public async Task<AppointmentResponseDto> CreateAppointmentAsync(CreateAppointmentDto dto, string clientId)
{
    var appointment = new Appointment
    {
        PatientId = clientId,
        DoctorId = int.Parse(dto.DoctorId),
        AppointmentDate = DateTime.Parse(dto.AppointmentDate),
        AppointmentTime = TimeSpan.Parse(dto.AppointmentTime),
        Reason = dto.Reason,
        Notes = dto.Notes,
        Status = AppointmentStatus.Scheduled
    };

    _context.Appointments.Add(appointment);
    await _context.SaveChangesAsync();
    
    return MapToDto(appointment);
}
```

---

## 4. COMPONENTA DE DATE (DATA LAYER)

### 4.1 Entity Framework Core și ApplicationDbContext

Stratul de date utilizează **Entity Framework Core 6.0** ca ORM (Object-Relational Mapping) pentru gestionarea bazei de date SQL Server.

```csharp
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    // DbSets pentru entitățile principale
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<Review> Reviews { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        ConfigureRelationships(builder);
    }
}
```

### 4.2 Modelarea Entităților de Domeniu

#### 4.2.1 Entitatea ApplicationUser (Extinde IdentityUser)

```csharp
public class ApplicationUser : IdentityUser
{
    [Required]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    public string LastName { get; set; } = string.Empty;
    
    public string DisplayName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public Gender? Gender { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

#### 4.2.2 Entitatea Appointment (Programări Medicale)

```csharp
public class Appointment
{
    public int Id { get; set; }
    
    [Required]
    public string PatientId { get; set; }
    public ApplicationUser Patient { get; set; }
    
    [Required]
    public int DoctorId { get; set; }
    public Doctor Doctor { get; set; }
    
    [Required]
    public DateTime AppointmentDate { get; set; }
    
    [Required]
    public TimeSpan AppointmentTime { get; set; }
    
    [Required]
    public string Reason { get; set; }
    
    public string? Notes { get; set; }
    
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}
```

#### 4.2.3 Configurarea Relațiilor în Entity Framework

```csharp
protected override void OnModelCreating(ModelBuilder builder)
{
    base.OnModelCreating(builder);

    // Configurare relații Doctor
    builder.Entity<Doctor>()
        .HasOne(d => d.User)
        .WithMany()
        .HasForeignKey(d => d.UserId)
        .OnDelete(DeleteBehavior.Restrict); // Evită cascading deletes

    // Configurare relații Appointment
    builder.Entity<Appointment>()
        .HasOne(a => a.Patient)
        .WithMany()
        .HasForeignKey(a => a.PatientId)
        .OnDelete(DeleteBehavior.Restrict);

    builder.Entity<Appointment>()
        .HasOne(a => a.Doctor)
        .WithMany()
        .HasForeignKey(a => a.DoctorId)
        .OnDelete(DeleteBehavior.Restrict);

    // Configurare relații Review
    builder.Entity<Review>()
        .HasOne(r => r.Client)
        .WithMany()
        .HasForeignKey(r => r.ClientId)
        .OnDelete(DeleteBehavior.Restrict);
}
```

### 4.3 Migrările Bazei de Date

Sistemul utilizează **Code-First Migrations** pentru versionarea schemei bazei de date:

```csharp
// Exemplu de migrare pentru crearea tabelei Doctors
public partial class CreateDoctorsTableClean : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Doctors' AND xtype='U')
            BEGIN
                CREATE TABLE [Doctors] (
                    [Id] int IDENTITY(1,1) NOT NULL,
                    [UserId] nvarchar(450) NOT NULL,
                    [Specialization] nvarchar(max) NOT NULL,
                    [Phone] nvarchar(max) NULL,
                    [IsActive] bit NOT NULL DEFAULT 1,
                    [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
                    CONSTRAINT [PK_Doctors] PRIMARY KEY ([Id])
                );
            END
        ");
    }
}
```

---

## 5. STRATUL DE ACCES LA DATE (REPOSITORY LAYER)

### 5.1 Implementarea Repository Pattern

Repository Pattern asigură abstractizarea accesului la date și permite testarea ușoară a logicii de business independent de baza de date.

#### 5.1.1 Interfața Repository

```csharp
public interface IAppointmentRepository
{
    // Operații CRUD de bază
    Task<AppointmentResponseDto> CreateAppointmentAsync(CreateAppointmentDto dto, string clientId);
    Task<List<AppointmentResponseDto>> GetClientAppointmentsAsync(string clientId);
    Task<List<AppointmentResponseDto>> GetAllAppointmentsAsync();
    
    // Operații pentru raportare și statistici
    Task<int> GetTotalAppointmentsCountAsync();
    Task<int> GetTodayAppointmentsCountAsync();
    Task<int> GetWeeklyAppointmentsCountAsync();
    Task<int> GetCompletedAppointmentsCountAsync();
    Task<int> GetAppointmentCountByStatusAsync(AppointmentStatus status);
}
```

#### 5.1.2 Implementarea Repository

```csharp
public class AppointmentRepository : IAppointmentRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AppointmentRepository> _logger;

    public AppointmentRepository(ApplicationDbContext context, ILogger<AppointmentRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<AppointmentResponseDto> CreateAppointmentAsync(CreateAppointmentDto dto, string clientId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var appointment = new Appointment
            {
                PatientId = clientId,
                DoctorId = int.Parse(dto.DoctorId),
                AppointmentDate = DateTime.Parse(dto.AppointmentDate),
                AppointmentTime = TimeSpan.Parse(dto.AppointmentTime),
                Reason = dto.Reason,
                Notes = dto.Notes,
                Status = AppointmentStatus.Scheduled,
                CreatedAt = DateTime.UtcNow
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return await MapToResponseDto(appointment);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error creating appointment for client {ClientId}", clientId);
            throw;
        }
    }
}
```

### 5.2 Avantajele Repository Pattern în MedanoClinic

#### 5.2.1 Abstractizarea Datelor

```
🗃️ BENEFICII REPOSITORY PATTERN
├── Încapsularea logicii de acces la date
├── Facilitarea unit testing-ului cu mock objects
├── Centralizarea query-urilor complexe
├── Posibilitatea de a schimba providerul de date
└── Respectarea principiului Single Responsibility
```

#### 5.2.2 Consistența Tranzacțiilor

Toate repository-urile implementează tranzacții pentru a asigura consistența datelor:

```csharp
public async Task<ReviewDto> CreateReviewAsync(CreateReviewDto dto, string clientId)
{
    using var transaction = await _context.Database.BeginTransactionAsync();
    try
    {
        // Verifică dacă programarea există și este completată
        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == dto.AppointmentId && a.PatientId == clientId);
            
        if (appointment?.Status != AppointmentStatus.Completed)
            throw new InvalidOperationException("Can only review completed appointments");

        var review = new Review
        {
            ClientId = clientId,
            DoctorId = dto.DoctorId,
            AppointmentId = dto.AppointmentId,
            Rating = dto.Rating,
            Comment = dto.Comment,
            CreatedAt = DateTime.UtcNow
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return MapToDto(review);
    }
    catch (Exception)
    {
        await transaction.RollbackAsync();
        throw;
    }
}
```

---

## 6. STRATUL DE LOGICĂ DE BUSINESS (SERVICE LAYER)

### 6.1 Arhitectura Serviciilor

Stratul de servicii conține întreaga logică de business și orchestrează operațiunile între diferite repository-uri și servicii externe.

#### 6.1.1 Serviciul Client - Orchestrarea Operațiunilor Pacienților

```csharp
public class ClientService : IClientService
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly IJobService _jobService;

    public async Task<AppointmentResponseDto> CreateAppointmentAsync(CreateAppointmentDto dto, string clientId)
    {
        // 1. Validări de business
        await ValidateAppointmentData(dto);
        
        // 2. Crează programarea prin repository
        var appointment = await _appointmentRepository.CreateAppointmentAsync(dto, clientId);
        
        // 3. Orchestrează serviciile auxiliare (notificări, job-uri)
        _jobService.SendAppointmentCreatedEmailJob(appointment);
        _jobService.ScheduleAppointmentReminder(appointment);
        
        // 4. Logging pentru audit
        _logger.LogInformation("Appointment created for client {ClientId}: {AppointmentId}", 
            clientId, appointment.Id);
        
        return appointment;
    }

    private async Task ValidateAppointmentData(CreateAppointmentDto dto)
    {
        // Validări specifice de business
        var appointmentDate = DateTime.Parse(dto.AppointmentDate);
        
        if (appointmentDate < DateTime.Today)
            throw new ArgumentException("Cannot schedule appointments in the past");
            
        if (appointmentDate.DayOfWeek == DayOfWeek.Sunday)
            throw new ArgumentException("Cannot schedule appointments on Sunday");
            
        // Verifică disponibilitatea doctorului
        var doctorAvailable = await _doctorRepository
            .IsDoctorAvailableAsync(int.Parse(dto.DoctorId), appointmentDate, TimeSpan.Parse(dto.AppointmentTime));
            
        if (!doctorAvailable)
            throw new InvalidOperationException("Doctor is not available at this time");
    }
}
```

#### 6.1.2 Serviciul Admin - Gestionarea Sistemului

```csharp
public class AdminService : IAdminService
{
    public async Task<AdminDashboardDto> GetDashboardDataAsync()
    {
        // Agregarea datelor din multiple surse pentru dashboard
        var dashboardData = new AdminDashboardDto
        {
            TotalUsers = await _userRepository.GetTotalUsersCountAsync(),
            TotalDoctors = await _doctorRepository.GetTotalDoctorsCountAsync(),
            TotalAppointments = await _appointmentRepository.GetTotalAppointmentsCountAsync(),
            TodayAppointments = await _appointmentRepository.GetTodayAppointmentsCountAsync(),
            WeeklyAppointments = await _appointmentRepository.GetWeeklyAppointmentsCountAsync(),
            CompletedAppointments = await _appointmentRepository.GetCompletedAppointmentsCountAsync(),
            
            // Statistici avansate
            AppointmentsByStatus = await GetAppointmentsByStatusAsync(),
            MonthlyTrends = await GetMonthlyTrendsAsync(),
            TopDoctors = await GetTopRatedDoctorsAsync()
        };

        return dashboardData;
    }
}
```

### 6.2 Serviciile de Infrastructură

#### 6.2.1 EmailService - Comunicarea cu Pacienții

```csharp
public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;
    private readonly UserManager<ApplicationUser> _userManager;

    public async Task<bool> SendEmailAsync(EmailNotificationDto emailDto)
    {
        try
        {
            // Validare adresă email
            if (!IsValidEmail(emailDto.ToEmail))
            {
                _logger.LogWarning("Invalid email format: {Email}", emailDto.ToEmail);
                emailDto.ToEmail = "test@example.com"; // Fallback
            }

            using var client = new SmtpClient(_emailSettings.SmtpHost, _emailSettings.SmtpPort)
            {
                Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword),
                EnableSsl = _emailSettings.EnableSsl
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                Subject = emailDto.Subject
            };

            // Suport pentru email multipart (HTML + Text)
            if (!string.IsNullOrEmpty(emailDto.HtmlBody) && !string.IsNullOrEmpty(emailDto.PlainTextBody))
            {
                var htmlView = AlternateView.CreateAlternateViewFromString(emailDto.HtmlBody, null, "text/html");
                var plainView = AlternateView.CreateAlternateViewFromString(emailDto.PlainTextBody, null, "text/plain");
                
                mailMessage.AlternateViews.Add(htmlView);
                mailMessage.AlternateViews.Add(plainView);
            }

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("✅ Email sent successfully to {Email}", emailDto.ToEmail);
            return true;
        }
        catch (SmtpException smtpEx)
        {
            _logger.LogError(smtpEx, "❌ SMTP Error sending email to {Email}", emailDto.ToEmail);
            return false;
        }
    }
}
```

#### 6.2.2 JobService - Automatizarea cu Hangfire

```csharp
public class JobService : IJobService
{
    private readonly IEmailService _emailService;
    private readonly ILogger<JobService> _logger;

    public void SendAppointmentCreatedEmailJob(AppointmentResponseDto appointment)
    {
        // Job imediat pentru confirmarea programării
        BackgroundJob.Enqueue(() => 
            _emailService.SendAppointmentCreatedEmailAsync(appointment));
    }

    public void ScheduleAppointmentReminder(AppointmentResponseDto appointment)
    {
        var appointmentDateTime = DateTime.Parse($"{appointment.AppointmentDate} {appointment.AppointmentTime}");
        var reminderTime = appointmentDateTime.AddHours(-1); // 1 oră înainte

        if (reminderTime > DateTime.Now)
        {
            BackgroundJob.Schedule(() => 
                _emailService.SendAppointmentReminderEmailAsync(appointment), 
                reminderTime);
                
            _logger.LogInformation("Scheduled reminder for appointment {AppointmentId} at {ReminderTime}", 
                appointment.Id, reminderTime);
        }
    }

    public void SetupRecurringJobs()
    {
        // Job recurent pentru curățarea datelor temporare
        RecurringJob.AddOrUpdate("cleanup-expired-tokens", 
            () => CleanupExpiredTokensAsync(), 
            Cron.Daily);

        // Job recurent pentru rapoarte zilnice
        RecurringJob.AddOrUpdate("daily-reports", 
            () => GenerateDailyReportsAsync(), 
            Cron.Daily(8)); // La ora 8:00 în fiecare zi
    }
}
```

---

## 7. STRATUL DE PREZENTARE (PRESENTATION LAYER)

### 7.1 Controllerele API

Stratul de prezentare expune API-ul REST prin controllerele ASP.NET Core, care gestionează request-urile HTTP și returnează response-urile corespunzătoare.

#### 7.1.1 AuthController - Autentificare și Autorizare

```csharp
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userMgr;
    private readonly JwtSettings _jwt;

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        try
        {
            // Validări de input
            if (dto == null || string.IsNullOrEmpty(dto.Email))
                return BadRequest("Email is required");

            // Creare utilizator
            var user = new ApplicationUser 
            {   
                UserName = dto.UserName,
                FirstName = dto.FirstName, 
                LastName = dto.LastName,
                Email = dto.Email,
                DisplayName = dto.DisplayName,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userMgr.CreateAsync(user, dto.Password);
            if (!result.Succeeded) 
                return BadRequest(result.Errors);

            // Atribuire rol implicit
            await _userMgr.AddToRoleAsync(user, "Client");
            
            return Ok(new { message = "User registered successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        // Verificare credențiale
        var user = await _userMgr.FindByEmailAsync(dto.Email);
        if (user == null || !await _userMgr.CheckPasswordAsync(user, dto.Password))
            return Unauthorized("Invalid credentials");

        // Obținere roluri
        var roles = await _userMgr.GetRolesAsync(user);
        var userRole = roles.FirstOrDefault() ?? "Client";
        
        // Generare JWT token
        var token = GenerateJwtToken(user, userRole);
        
        return new AuthResponseDto
        {
            Email = user.Email,
            Role = userRole,
            Token = token,
            Expiry = DateTime.UtcNow.AddMinutes(_jwt.ExpiryMinutes)
        };
    }
}
```

#### 7.1.2 ClientController - Operațiuni pentru Pacienți

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Client")]
public class ClientController : ControllerBase
{
    private readonly IClientService _clientService;

    [HttpGet("doctors")]
    public async Task<ActionResult<List<DoctorDto>>> GetDoctors()
    {
        var doctors = await _clientService.GetDoctorsAsync();
        return Ok(doctors);
    }

    [HttpPost("appointments")]
    public async Task<ActionResult<AppointmentResponseDto>> CreateAppointment(CreateAppointmentDto dto)
    {
        try
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var appointment = await _clientService.CreateAppointmentAsync(dto, clientId);
            return CreatedAtAction(nameof(GetAppointments), new { id = appointment.Id }, appointment);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpGet("appointments")]
    public async Task<ActionResult<List<AppointmentResponseDto>>> GetAppointments()
    {
        var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var appointments = await _clientService.GetClientAppointmentsAsync(clientId);
        return Ok(appointments);
    }

    [HttpPost("reviews")]
    public async Task<ActionResult<ReviewDto>> CreateReview(CreateReviewDto dto)
    {
        var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var review = await _clientService.CreateReviewAsync(dto, clientId);
        return CreatedAtAction(nameof(GetReviews), new { id = review.Id }, review);
    }
}
```

### 7.2 Data Transfer Objects (DTOs)

DTOs asigură transferul securizat al datelor între straturi și oferă control asupra informațiilor expuse către exterior.

#### 7.2.1 Request DTOs

```csharp
public class CreateAppointmentDto
{
    [Required]
    public string DoctorId { get; set; } // String pentru compatibilitate frontend
    
    [Required]
    public string AppointmentDate { get; set; }
    
    [Required]
    public string AppointmentTime { get; set; }
    
    [Required]
    public string Reason { get; set; }
    
    public string? Notes { get; set; }
}

public class RegisterDto
{
    public string UserName { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string DisplayName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public Gender? Gender { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}
```

#### 7.2.2 Response DTOs

```csharp
public class AppointmentResponseDto
{
    public string Id { get; set; }
    public string ClientId { get; set; }
    public string ClientName { get; set; }
    public string DoctorId { get; set; }
    public string DoctorName { get; set; }
    public string DoctorSpecialization { get; set; }
    public string AppointmentDate { get; set; }
    public string AppointmentTime { get; set; }
    public string Reason { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; }
    public string CreatedAt { get; set; }
}

public class AuthResponseDto
{
    public string Email { get; set; }
    public string Role { get; set; }
    public string Token { get; set; }
    public DateTime Expiry { get; set; }
}
```

---

## 8. SISTEMUL DE SECURITATE ȘI AUTENTIFICARE

### 8.1 Implementarea JWT (JSON Web Tokens)

Sistemul utilizează JWT pentru autentificare stateless și scalabilă.

#### 8.1.1 Configurarea JWT în Program.cs

```csharp
// Configurarea JWT Authentication
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
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero // Reduce token expiry tolerance
    };
});
```

#### 8.1.2 Generarea JWT Token

```csharp
private string GenerateJwtToken(ApplicationUser user, string role)
{
    var claims = new List<Claim> 
    {
        new(JwtRegisteredClaimNames.Sub, user.Id),
        new(JwtRegisteredClaimNames.Email, user.Email),
        new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new(ClaimTypes.Role, role)
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Secret));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    var expires = DateTime.UtcNow.AddMinutes(_jwt.ExpiryMinutes);

    var token = new JwtSecurityToken(
        issuer: _jwt.Issuer,
        audience: _jwt.Audience,
        claims: claims,
        expires: expires,
        signingCredentials: creds
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}
```

### 8.2 Role-Based Authorization

Sistemul implementează autorizarea bazată pe roluri cu trei roluri principale:

#### 8.2.1 Definirea Rolurilor

```csharp
public static class Roles
{
    public const string Admin = "Admin";
    public const string Doctor = "Doctor";  
    public const string Client = "Client";
}

// Utilizarea în controlere
[Authorize(Roles = "Client")]
public class ClientController : ControllerBase { }

[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase { }
```

#### 8.2.2 Seed-area Rolurilor la Startup

```csharp
// În Program.cs - Seed roles
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
```

### 8.3 Securitatea la Nivel de Transport

#### 8.3.1 HTTPS și CORS

```csharp
// CORS Configuration pentru frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://medanoclinic.com")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// HTTPS Redirection în pipeline
app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
```

---

## 9. INFRASTRUCTURA ȘI SERVICII TRANSVERSALE

### 9.1 Hangfire pentru Background Jobs

Hangfire gestionează job-urile asincrone și programate pentru notificări și task-uri repetitive.

#### 9.1.1 Configurarea Hangfire

```csharp
// Configurare Hangfire cu SQL Server Storage
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"), 
        new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.Zero,
            UseRecommendedIsolationLevel = true,
            DisableGlobalLocks = true
        }));

// Adăugare Hangfire Server
builder.Services.AddHangfireServer(options =>
{
    options.SchedulePollingInterval = TimeSpan.FromSeconds(30);
    options.Queues = new[] { "notifications", "default" };
});
```

#### 9.1.2 Tipuri de Job-uri Implementate

```csharp
public class JobService : IJobService
{
    // Fire-and-forget jobs
    public void SendAppointmentCreatedEmailJob(AppointmentResponseDto appointment)
    {
        BackgroundJob.Enqueue(() => 
            _emailService.SendAppointmentCreatedEmailAsync(appointment));
    }

    // Delayed jobs
    public void ScheduleAppointmentReminder(AppointmentResponseDto appointment)
    {
        var appointmentDateTime = DateTime.Parse($"{appointment.AppointmentDate} {appointment.AppointmentTime}");
        var reminderTime = appointmentDateTime.AddHours(-1);

        BackgroundJob.Schedule(() => 
            _emailService.SendAppointmentReminderEmailAsync(appointment), 
            reminderTime);
    }

    // Recurring jobs
    public void SetupRecurringJobs()
    {
        RecurringJob.AddOrUpdate("cleanup-expired-sessions", 
            () => CleanupExpiredSessionsAsync(), 
            Cron.Daily);

        RecurringJob.AddOrUpdate("generate-daily-reports", 
            () => GenerateDailyReportsAsync(), 
            Cron.Daily(8));

        RecurringJob.AddOrUpdate("send-weekly-summaries", 
            () => SendWeeklySummariesAsync(), 
            Cron.Weekly(DayOfWeek.Monday, 9));
    }
}
```

### 9.2 Logging și Monitoring

#### 9.2.1 Structured Logging cu Serilog

```csharp
// Configurare logging în Program.cs
builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .WriteTo.Console()
        .WriteTo.File("logs/medanoclinic-.log", 
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30)
        .WriteTo.Seq("http://localhost:5341") // Pentru development
        .Enrich.WithProperty("Application", "MedanoClinic")
        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName);
});

// Utilizare în servicii
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public async Task<bool> SendEmailAsync(EmailNotificationDto emailDto)
    {
        _logger.LogInformation("Attempting to send email to {Email} with subject: {Subject}", 
            emailDto.ToEmail, emailDto.Subject);
            
        try
        {
            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("✅ Email sent successfully to {Email}", emailDto.ToEmail);
            return true;
        }
        catch (SmtpException smtpEx)
        {
            _logger.LogError(smtpEx, "❌ SMTP Error sending email to {Email}. StatusCode: {StatusCode}", 
                emailDto.ToEmail, smtpEx.StatusCode);
            return false;
        }
    }
}
```

### 9.3 Health Checks

```csharp
// Configurare Health Checks
builder.Services.AddHealthChecks()
    .AddDbContext<ApplicationDbContext>()
    .AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
    .AddSmtpHealthCheck(options =>
    {
        options.Host = builder.Configuration["EmailSettings:SmtpHost"];
        options.Port = int.Parse(builder.Configuration["EmailSettings:SmtpPort"]);
    })
    .AddHangfire(options => 
    {
        options.MaximumJobsFailed = 5;
        options.MinimumAvailableServers = 1;
    });

// Endpoint pentru health checks
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
```

---

## 10. DEPENDENȚELE ȘI INJECȚIA DE DEPENDENȚE

### 10.1 Configurarea Container-ului DI

Sistemul utilizează containerul de DI integrat în .NET 6 pentru gestionarea dependențelor:

```csharp
// Program.cs - Înregistrarea serviciilor în container
public void ConfigureServices(WebApplicationBuilder builder)
{
    // 1. Entity Framework + Identity
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
    
    builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

    // 2. Repository Pattern
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
    builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
    builder.Services.AddScoped<IReviewRepository, ReviewRepository>();

    // 3. Business Services
    builder.Services.AddScoped<IAdminService, AdminService>();
    builder.Services.AddScoped<IClientService, ClientService>();

    // 4. Infrastructure Services
    builder.Services.AddScoped<IEmailService, EmailService>();
    builder.Services.AddScoped<INotificationService, NotificationService>();
    builder.Services.AddScoped<IJobService, JobService>();

    // 5. Configuration Objects
    builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
    builder.Services.AddSingleton<JwtSettings>(builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>());
}
```

### 10.2 Avantajele Injecției de Dependențe

#### 10.2.1 Loose Coupling (Cuplare Slabă)

```csharp
// Service-ul depinde de abstracții, nu de implementări concrete
public class ClientService : IClientService
{
    // Toate dependențele sunt injectate prin constructor
    private readonly IDoctorRepository _doctorRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IEmailService _emailService;
    private readonly IJobService _jobService;

    public ClientService(
        IDoctorRepository doctorRepository,
        IAppointmentRepository appointmentRepository,
        IEmailService emailService,
        IJobService jobService)
    {
        _doctorRepository = doctorRepository ?? throw new ArgumentNullException(nameof(doctorRepository));
        _appointmentRepository = appointmentRepository ?? throw new ArgumentNullException(nameof(appointmentRepository));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _jobService = jobService ?? throw new ArgumentNullException(nameof(jobService));
    }
}
```

#### 10.2.2 Testabilitate Sporită

```csharp
// În testele unitare, dependențele pot fi mock-uite ușor
[Test]
public async Task CreateAppointment_ShouldSendNotification_WhenAppointmentIsCreated()
{
    // Arrange
    var mockAppointmentRepo = new Mock<IAppointmentRepository>();
    var mockJobService = new Mock<IJobService>();
    var mockDoctorRepo = new Mock<IDoctorRepository>();
    var mockEmailService = new Mock<IEmailService>();

    var clientService = new ClientService(
        mockDoctorRepo.Object,
        mockAppointmentRepo.Object,
        mockEmailService.Object,
        mockJobService.Object);

    var appointmentDto = new CreateAppointmentDto { /* test data */ };

    // Act
    var result = await clientService.CreateAppointmentAsync(appointmentDto, "client123");

    // Assert
    mockJobService.Verify(x => x.SendAppointmentCreatedEmailJob(It.IsAny<AppointmentResponseDto>()), 
        Times.Once);
}
```

### 10.3 Lifetime Management

Sistemul utilizează trei tipuri de lifetime pentru servicii:

```csharp
// Singleton - O singură instanță pentru întreaga aplicație
builder.Services.AddSingleton<JwtSettings>();

// Scoped - O instanță per HTTP request
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<ApplicationDbContext>();

// Transient - O instanță nouă la fiecare injectare
builder.Services.AddTransient<IEmailValidator, EmailValidator>();
```

---

## 11. GESTIONAREA CONFIGURĂRILOR

### 11.1 Structura Configurărilor

Aplicația utilizează sistemul de configurare ierarhic al .NET pentru gestionarea setărilor:

```json
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MedanoClinicDB;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "JwtSettings": {
    "Secret": "YourSuperSecretKeyForJWTTokenGeneration",
    "Issuer": "MedanoClinic",
    "Audience": "MedanoClinic-Users",
    "ExpiryMinutes": 60
  },
  "EmailSettings": {
    "SmtpHost": "sandbox.smtp.mailtrap.io",
    "SmtpPort": 2525,
    "SmtpUsername": "your_username",
    "SmtpPassword": "your_password", 
    "EnableSsl": true,
    "FromEmail": "noreply@medanoclinic.com",
    "FromName": "MedanoClinic"
  },
  "HangfireSettings": {
    "Dashboard": {
      "Enabled": true,
      "Path": "/hangfire"
    },
    "JobQueues": ["notifications", "reports", "default"]
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  }
}
```

### 11.2 Configuration Classes (Strongly-Typed Configuration)

#### 11.2.1 EmailSettings Configuration

```csharp
public class EmailSettings
{
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public bool EnableSsl { get; set; }
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    
    // Validare configurație
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(SmtpHost) && 
               SmtpPort > 0 && 
               !string.IsNullOrEmpty(SmtpUsername) &&
               !string.IsNullOrEmpty(FromEmail);
    }
}
```

#### 11.2.2 JWT Settings Configuration

```csharp
public class JwtSettings
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; } = 60;
    
    // Validare securitate
    public void ValidateSettings()
    {
        if (string.IsNullOrEmpty(Secret) || Secret.Length < 32)
            throw new InvalidOperationException("JWT Secret must be at least 32 characters long");
            
        if (ExpiryMinutes <= 0)
            throw new InvalidOperationException("JWT Expiry must be positive");
    }
}
```

### 11.3 Environment-Specific Configuration

```csharp
// appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "EmailSettings": {
    "SmtpHost": "localhost",
    "SmtpPort": 1025, // MailHog pentru development
    "EnableSsl": false
  }
}

// appsettings.Production.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  },
  "EmailSettings": {
    "SmtpHost": "smtp.production.com",
    "SmtpPort": 587,
    "EnableSsl": true
  }
}
```

---

## 12. PATTERNURI DE DESIGN IMPLEMENTATE

### 12.1 Repository Pattern

**Scop**: Abstractizarea accesului la date și centralizarea query-urilor.

**Implementare**:
```csharp
public interface IAppointmentRepository
{
    Task<AppointmentResponseDto> CreateAppointmentAsync(CreateAppointmentDto dto, string clientId);
    Task<List<AppointmentResponseDto>> GetClientAppointmentsAsync(string clientId);
    Task<bool> UpdateAppointmentStatusAsync(int appointmentId, AppointmentStatus status);
}
```

**Beneficii**:
- Testabilitate sporită prin mock-uri
- Consistența tranzacțiilor
- Posibilitatea schimbării providerului de date

### 12.2 Service Layer Pattern

**Scop**: Încapsularea logicii de business și orchestrarea operațiilor complexe.

**Implementare**:
```csharp
public class ClientService : IClientService
{
    // Orchestrează operațiuni din multiple repository-uri
    public async Task<AppointmentResponseDto> CreateAppointmentAsync(CreateAppointmentDto dto, string clientId)
    {
        // 1. Validări business
        await ValidateAppointmentBusinessRules(dto);
        
        // 2. Operațiuni de date
        var appointment = await _appointmentRepository.CreateAppointmentAsync(dto, clientId);
        
        // 3. Declanșare servicii auxiliare
        _jobService.SendAppointmentCreatedEmailJob(appointment);
        _jobService.ScheduleAppointmentReminder(appointment);
        
        return appointment;
    }
}
```

### 12.3 Factory Pattern pentru Email Templates

**Scop**: Crearea dinamică de template-uri de email bazate pe tip.

**Implementare**:
```csharp
public class EmailTemplateFactory : IEmailTemplateFactory
{
    public EmailTemplate CreateTemplate(NotificationType type, AppointmentResponseDto appointment)
    {
        return type switch
        {
            NotificationType.AppointmentCreated => new AppointmentCreatedTemplate(appointment),
            NotificationType.AppointmentModified => new AppointmentModifiedTemplate(appointment),
            NotificationType.AppointmentReminder => new AppointmentReminderTemplate(appointment),
            NotificationType.AppointmentCancelled => new AppointmentCancelledTemplate(appointment),
            _ => throw new ArgumentException($"Unsupported notification type: {type}")
        };
    }
}
```

### 12.4 Command Pattern pentru Background Jobs

**Scop**: Încapsularea operațiunilor asincrone în obiecte executabile.

**Implementare**:
```csharp
public class SendEmailCommand : ICommand
{
    private readonly EmailNotificationDto _emailDto;
    private readonly IEmailService _emailService;

    public async Task ExecuteAsync()
    {
        await _emailService.SendEmailAsync(_emailDto);
    }
}

// Utilizare în JobService
public void EnqueueEmailCommand(EmailNotificationDto emailDto)
{
    BackgroundJob.Enqueue<SendEmailCommand>(command => command.ExecuteAsync());
}
```

### 12.5 Builder Pattern pentru Configurări Complexe

**Scop**: Construirea pas cu pas a obiectelor complexe de configurare.

**Implementare**:
```csharp
public class EmailConfigurationBuilder
{
    private EmailConfiguration _config = new();

    public EmailConfigurationBuilder WithSmtpSettings(string host, int port)
    {
        _config.SmtpHost = host;
        _config.SmtpPort = port;
        return this;
    }

    public EmailConfigurationBuilder WithCredentials(string username, string password)
    {
        _config.SmtpUsername = username;
        _config.SmtpPassword = password;
        return this;
    }

    public EmailConfigurationBuilder WithSsl(bool enableSsl)
    {
        _config.EnableSsl = enableSsl;
        return this;
    }

    public EmailConfiguration Build()
    {
        _config.Validate();
        return _config;
    }
}
```

---

## CONCLUZIE ARHITECTURALĂ

### Punctele Forte ale Arhitecturii MedanoClinic

1. **🏗️ Clean Architecture**: Separarea clară a responsabilităților și independența straturilor
2. **🔧 SOLID Principles**: Respectarea principiilor de design orientat-obiect
3. **🧪 Testabilitate**: Arhitectură care facilitează unit testing și integration testing
4. **📈 Scalabilitate**: Design pregătit pentru creșterea volumului de date și utilizatori
5. **🔒 Securitate**: Implementarea JWT, role-based authorization și validări multiple
6. **⚡ Performance**: Utilizarea async/await, connection pooling și optimizări de query-uri
7. **📊 Observability**: Logging structurat, health checks și monitoring

### Prepararea pentru Extinderi Viitoare

Arhitectura actuală permite extinderi ușoare pentru:
- **Microservicii**: Fiecare service poate deveni un microserviciu independent
- **Event-Driven Architecture**: Integrarea cu message brokers (RabbitMQ, Azure Service Bus)
- **CQRS**: Separarea comenzilor de query-uri pentru performanță optimă
- **Cloud Deployment**: Compatibilitate nativă cu Azure, AWS, Google Cloud

Această arhitectură reprezintă o fundație solidă pentru un sistem de management medical modern, scalabil și mentenabil, demonstrând aplicarea practică a principiilor software engineering-ului în contextul unei aplicații medicale reale.- 