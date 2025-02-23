# Platform Project
https://youtu.be/WidSNb51Z5k 
## Overview

This is a .NET 9.0 web application for managing coaches, courses, and bookings. It includes authentication, admin validation, and email services.

## Features

- **User authentication** (Login, Registration, Forgot Password)
- **Coach management** (Apply to become a coach, Book sessions)
- **Course management** (Add courses, Purchase courses)
- **Admin dashboard** (Validate coaches and courses)
- **Email notifications** using SMTP

## Technologies Used

- **Backend:** C# ASP.NET Core 9.0, Entity Framework Core 
- **Frontend:** Razor Views (.cshtml), Bootstrap
- **Database:** SQLite
- **Authentication:** Identity Framework
- **Email Service:** SMTP

## Project Structure

- Controllers/ **MVC** Account/Home/Admin/Coaches/Courses **Controllers**
- Models/  **Entity and ViewModels**
- Views/ **Razor** views
- Services/Email/ **Email** service implementation
- Migrations/ **EF Core** migrations
- wwwroot/ **Static assets** (CSS, JS, Images)

## Installation & Setup

### Prerequisites

- .NET 9.0 SDK
- SQLite (Bundled with EF Core)
- SMTP credentials for email service (if needed)

### Steps

1. **Clone the repository**

```
git clone <repository-url>
cd platform-project
```

2. **Restore dependencies**

```
dotnet restore
```

3. **Apply database migrations**

```
dotnet ef database update
```

4. **Run the application**

```
dotnet run
```

5. Open in browser

```
http://localhost:5000
```
