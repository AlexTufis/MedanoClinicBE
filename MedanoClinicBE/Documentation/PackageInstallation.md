# ?? Package Installation Guide

## Required NuGet Packages

To complete the notification system implementation, install these packages:

### Core Packages (Already included in .NET 6)
```bash
# These are already included in your project
System.Net.Mail
System.Net.Security
Microsoft.Extensions.Options.ConfigurationExtensions
```

### Optional: Production Scheduling (Choose one)

#### Option 1: Hangfire (Recommended for SQL Server)
```bash
dotnet add package Hangfire
dotnet add package Hangfire.SqlServer
```

#### Option 2: Quartz.NET (More features, complex setup)
```bash
dotnet add package Quartz
dotnet add package Quartz.Extensions.Hosting
```

### Optional: Enhanced Features

#### For Real-time Notifications
```bash
dotnet add package Microsoft.AspNetCore.SignalR
```

#### For Advanced Email Templates
```bash
dotnet add package RazorLight  # For Razor templates
# OR
dotnet add package Scriban     # For Scriban templates
```

#### For SMS Notifications
```bash
dotnet add package Twilio
```

## Manual Installation Steps

Since your environment has .NET SDK issues, you can manually add these to your `.csproj` file:

### Update MedanoClinicBE.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="6.0.0" />
    <PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="6.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="6.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="6.0.0" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.2.3" />
    
    <!-- Optional: Add Hangfire for production -->
    <!-- <PackageReference Include="Hangfire" Version="1.8.0" /> -->
    <!-- <PackageReference Include="Hangfire.SqlServer" Version="1.8.0" /> -->
    
    <!-- Optional: Add SignalR for real-time notifications -->
    <!-- <PackageReference Include="Microsoft.AspNetCore.SignalR" Version="6.0.0" /> -->
  </ItemGroup>

</Project>
```

## Current Implementation Features

? **Working without additional packages:**
- Email notifications using System.Net.Mail
- In-app notifications with in-memory storage
- Timer-based appointment reminders
- Professional HTML email templates
- REST API for notification management

? **Ready for production with package installation:**
- Hangfire for robust job scheduling
- SignalR for real-time notifications
- Database storage for notifications
- Advanced template engines