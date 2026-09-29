# 🎓 Scholarship Platform API

A backend RESTful API built to power scholarship management, application workflows, payments, and student-mentor feedback loops. Built on **.NET 10** using **ASP.NET Core Minimal APIs**, **Entity Framework Core**, and **PostgreSQL**, structured around clean **Vertical Slice / Feature-based Architecture**.

---

## Key Features

* **Authentication & Authorization:** JWT-based authentication with role support (students, reviewers, admins).
* **Scholarship Management:** Endpoints for publishing, filtering, and applying to scholarship programs.
* **Notifications & Email:** Services and endpoints to send emails and store user notifications.
* **Document & Asset Storage:** Handling file attachments for application submissions.
* **Payment Records:** Tracking stipend and disbursement histories.
* **Feedback System:** Reviewer notes and evaluations tied to specific applications.

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
├── Users/                    # User accounts and roles
├── Scholarships/             # Scholarship listings and details
├── ScholarshipApplications/  # Application submissions and status handling
├── Notifications/            # System notifications
├── Email/                    # Transactional email service
├── Payments/                 # Payment tracking
├── Courses/                  # Course data and relations
├── Feedbacks/                # Application feedback
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


5. **Test endpoints:**
Open `https://localhost:<PORT>/scalar/v1` or `/swagger` in your browser.

---

## Main Endpoints

* `POST /api/auth/login` – User authentication & token issuance
* `GET  /api/scholarships` – List active scholarship listings
* `POST /api/notifications` – Internal notification triggers
* `GET  /api/payments` – Track payment records

---

## 👤 Author

* **Lucas Katalahali**
* [LinkedIn](https://www.linkedin.com/in/lucas-katalahali/)
* [GitHub](https://github.com/Lucaskatalahali)
