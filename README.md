# 🎓 Scholarship Mentorship API

A backend RESTful API designed to manage scholarship applications, courses, mentee payments, public feedback, and email/in-app notifications. Built with **.NET 10** using **ASP.NET Core Minimal APIs**, **Entity Framework Core**, and **PostgreSQL**.

The project uses a **feature-based folder structure** to keep endpoints, models, and business logic grouped by domain.

---

## Features

* **Authentication & Authorization:** Secure user registration, authentication, and role management (Applicant, Mentor, Admin) using ASP.NET Core Identity and JWT tokens.
* **Data Validation:** Request validation powered by **FluentValidation** to ensure domain integrity before hitting service layers.
* **Relational Persistence:** Code-First database design and migrations managed through **Entity Framework Core** and **PostgreSQL**.
* **Clean Architecture Practices:** Structured Data Transfer Objects (DTOs), dedicated service handlers, and standardized error responses.
* **Scholarship Management:** Endpoints for browsing, filtering, and applying to scholarship programs.
* **Courses:** Endpoints for managing available courses and related academic details.
* **Payment Tracking:** Recording and tracking fees paid by mentees.
* **Public Feedback:** Open endpoints allowing visitors and platform users to submit feedback and suggestions.
* **Notifications & Email:** Services and endpoints to send emails and store user notifications.
* **Document & File Storage:** Handling file uploads and attachments for application submissions.

---

## Tech Stack

* **Framework:** .NET 10 (`net10.0`) / ASP.NET Core Minimal APIs
* **Database:** PostgreSQL
* **ORM:** Entity Framework Core
* **Security:** ASP.NET Core Identity & JWT Bearer
* **Docs/Testing:** OpenAPI (Swagger / Scalar)

---

## Project Structure

The project avoids generic `Controllers/` and `Models/` folders, keeping code organized by feature:

```
ScholarshipPlatform/
├── Authentication/           # Login, registration, and token handling
├── Users/                    # User accounts and profile data
├── Scholarships/             # Scholarship listings and details
├── ScholarshipApplications/  # Application submissions and status handling
├── Courses/                  # Courses and educational programs
├── Payments/                 # Mentee payment records
├── Feedbacks/                # Public feedback and submission handling
├── Notifications/            # User notification endpoints and logic
├── Email/                    # Email delivery service
├── Storage/                  # Upload and asset handling
├── Common/                   # Shared utilities and base classes
├── Data/                     # DbContext and database mappings
├── Migrations/               # EF Core database migrations
├── Program.cs                # App configuration and endpoint registration
└── appsettings.json          # App configuration

```

---

## Getting Started

### Prerequisites

* [.NET 10 SDK](https://dotnet.microsoft.com/download)
* [PostgreSQL](https://www.postgresql.org/)

### Setup

Clone the repository
Update `appsettings.Development.json` with your connection string and credentials:
Apply database migrations: dotnet ef database update
Run the API: dotnet run

**Test endpoints:**
Open `https://localhost:<PORT>/scalar/v1` or `/swagger` in your browser.

---

## Main Endpoints

* `POST /api/auth/login` – User authentication & token issuance
* `GET  /api/scholarships` – List scholarship listings
* `POST /api/notifications` – Internal notification triggers
* `GET  /api/payments` – Track payment records

---

## 👤 Author

* **Lucas Katalahali**
* [LinkedIn](https://www.linkedin.com/in/lucas-katalahali/)
* [GitHub](https://github.com/Lucaskatalahali)
